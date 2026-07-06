using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace OsNotifications;

internal static class WindowsApplicationRegistration {
	private const int MaxPath = 260;

	[DllImport("shell32.dll", SetLastError = true)]
	private static extern void SetCurrentProcessExplicitAppUserModelID(
		[MarshalAs(UnmanagedType.LPWStr)] string appId);

	public static void Register(string? applicationName, string? applicationId) {
		using Process process = Process.GetCurrentProcess();
		string executablePath = process.MainModule?.FileName
			?? throw new InvalidOperationException("Unable to resolve current process executable path.");

		string name = string.IsNullOrWhiteSpace(applicationName)
			? Path.GetFileNameWithoutExtension(executablePath)
			: applicationName.Trim();

		string appUserModelId = string.IsNullOrWhiteSpace(applicationId)
			? name
			: applicationId.Trim();

		SetCurrentProcessExplicitAppUserModelID(appUserModelId);

		string programsPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			@"Microsoft\Windows\Start Menu\Programs");

		Directory.CreateDirectory(programsPath);

		using ShellShortcut shortcut = new ShellShortcut {
			TargetPath = executablePath,
			Arguments = string.Empty,
			AppUserModelId = appUserModelId
		};

		shortcut.Save(Path.Combine(programsPath, $"{name}.lnk"));
	}

	private sealed class ShellShortcut : IDisposable {
		private const int MaxArguments = 1024;
		private PropertyKey _appUserModelIdKey = new("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}", 5);
		private IShellLinkW? _shellLink = (IShellLinkW)new CShellLink();

		public string TargetPath {
			get {
				StringBuilder targetPath = new(MaxPath);
				Win32FindData data = new();

				VerifySucceeded(_shellLink!.GetPath(targetPath, targetPath.Capacity, ref data, 0));
				return targetPath.ToString();
			}
			set => VerifySucceeded(_shellLink!.SetPath(value));
		}

		public string Arguments {
			get {
				StringBuilder arguments = new(MaxArguments);

				VerifySucceeded(_shellLink!.GetArguments(arguments, arguments.Capacity));
				return arguments.ToString();
			}
			set => VerifySucceeded(_shellLink!.SetArguments(value));
		}

		public string AppUserModelId {
			set {
				using PropVariant propertyValue = new(value);

				VerifySucceeded(PropertyStore.SetValue(ref _appUserModelIdKey, propertyValue));
				VerifySucceeded(PropertyStore.Commit());
			}
		}

		private IPersistFile PersistFile => _shellLink is IPersistFile persistFile
			? persistFile
			: throw new COMException("Unable to create IPersistFile for shell shortcut.");

		private IPropertyStore PropertyStore => _shellLink is IPropertyStore propertyStore
			? propertyStore
			: throw new COMException("Unable to create IPropertyStore for shell shortcut.");

		public void Save(string path) => PersistFile.Save(path, true);

		public void Dispose() {
			if (_shellLink == null)
				return;

			Marshal.FinalReleaseComObject(_shellLink);
			_shellLink = null;
		}

		private static void VerifySucceeded(uint hresult) {
			if (hresult > 1)
				Marshal.ThrowExceptionForHR((int)hresult);
		}
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("000214F9-0000-0000-C000-000000000046")]
	private interface IShellLinkW {
		uint GetPath(
			[Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
			int cchMaxPath,
			ref Win32FindData pfd,
			uint fFlags);

		uint GetIDList(out IntPtr ppidl);
		uint SetIDList(IntPtr pidl);
		uint GetDescription([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
		uint SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
		uint GetWorkingDirectory([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
		uint SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
		uint GetArguments([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
		uint SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
		uint GetHotKey(out ushort pwHotkey);
		uint SetHotKey(ushort wHotKey);
		uint GetShowCmd(out int piShowCmd);
		uint SetShowCmd(int iShowCmd);
		uint GetIconLocation([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
		uint SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
		uint SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
		uint Resolve(IntPtr hwnd, uint fFlags);
		uint SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
	}

	[ComImport]
	[ClassInterface(ClassInterfaceType.None)]
	[Guid("00021401-0000-0000-C000-000000000046")]
	private class CShellLink {
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
	private interface IPropertyStore {
		uint GetCount(out uint cProps);
		uint GetAt(uint iProp, out PropertyKey pkey);
		uint GetValue(ref PropertyKey key, PropVariant pv);
		uint SetValue(ref PropertyKey key, PropVariant pv);
		uint Commit();
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	private struct PropertyKey {
		public PropertyKey(string formatId, int propertyId) {
			FormatId = new Guid(formatId);
			PropertyId = propertyId;
		}

		public Guid FormatId { get; }
		public int PropertyId { get; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
	private struct Win32FindData {
		public uint FileAttributes;
		public FILETIME CreationTime;
		public FILETIME LastAccessTime;
		public FILETIME LastWriteTime;
		public uint FileSizeHigh;
		public uint FileSizeLow;
		public uint Reserved0;
		public uint Reserved1;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxPath)]
		public string FileName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
		public string AlternateFileName;
	}

	[StructLayout(LayoutKind.Explicit)]
	private sealed class PropVariant : IDisposable {
		[FieldOffset(0)]
		private ushort _valueType;

		[FieldOffset(8)]
		private IntPtr _value;

		public PropVariant(string value) {
			_valueType = (ushort)VarEnum.VT_LPWSTR;
			_value = Marshal.StringToCoTaskMemUni(value);
		}

		~PropVariant() {
			Dispose();
		}

		public void Dispose() {
			PropVariantClear(this);
			GC.SuppressFinalize(this);
		}
	}

	[DllImport("Ole32.dll", PreserveSig = false)]
	private static extern void PropVariantClear([In][Out] PropVariant pvar);
}

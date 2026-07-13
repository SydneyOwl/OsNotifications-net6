#import <UserNotifications/UserNotifications.h>

#define NOTIFICATION_TIMEOUT_SECONDS 30

// --- Delegate to allow notifications while app is in foreground ---
@interface MacNotificationDelegate : NSObject <UNUserNotificationCenterDelegate>
@end

@implementation MacNotificationDelegate
- (void)userNotificationCenter:(UNUserNotificationCenter *)center
       willPresentNotification:(UNNotification *)notification
         withCompletionHandler:(void (^)(UNNotificationPresentationOptions))completionHandler {
    completionHandler(UNNotificationPresentationOptionBanner
                    | UNNotificationPresentationOptionSound
                    | UNNotificationPresentationOptionBadge);
}
@end

void initializeNotificationDelegate(void) {
    static MacNotificationDelegate *delegate = nil;
    if (!delegate) {
        delegate = [[MacNotificationDelegate alloc] init];
        [UNUserNotificationCenter currentNotificationCenter].delegate = delegate;
    }
}

// --- Unified callback type ---
typedef void (*MacNotificationCallback)(int32_t result, void *userData);

// --- Async (callback-based, no global state) ---

void requestNotificationPermissionAsync(MacNotificationCallback callback, void *userData) {
    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    UNAuthorizationOptions options = UNAuthorizationOptionAlert
                                   | UNAuthorizationOptionSound
                                   | UNAuthorizationOptionBadge;
    [center requestAuthorizationWithOptions:options
                          completionHandler:^(BOOL granted, NSError *error) {
        int32_t result = error ? -1 : (granted ? 1 : 0);
        if (callback) callback(result, userData);
    }];
}

void getNotificationPermissionStatusAsync(MacNotificationCallback callback, void *userData) {
    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    [center getNotificationSettingsWithCompletionHandler:^(UNNotificationSettings *settings) {
        int32_t status = (int32_t)settings.authorizationStatus;
        if (callback) callback(status, userData);
    }];
}

void showNotificationAsync(const char *title, const char *subtitle, const char *body,
                           MacNotificationCallback callback, void *userData) {
    UNMutableNotificationContent *content = [[UNMutableNotificationContent alloc] init];
    if (title)    content.title    = [NSString stringWithUTF8String:title];
    if (subtitle) content.subtitle = [NSString stringWithUTF8String:subtitle];
    if (body)     content.body     = [NSString stringWithUTF8String:body];

    NSString *identifier = [[NSUUID UUID] UUIDString];
    UNNotificationRequest *request = [UNNotificationRequest requestWithIdentifier:identifier
                                                                         content:content
                                                                         trigger:nil];

    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    [center addNotificationRequest:request withCompletionHandler:^(NSError *error) {
        if (callback) callback(error ? -1 : 0, userData);
    }];
}

// --- Sync (semaphore-based, with timeout) ---

int32_t getNotificationPermissionStatus() {
    dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
    __block int32_t status = 0;

    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    [center getNotificationSettingsWithCompletionHandler:^(UNNotificationSettings *settings) {
        status = (int32_t)settings.authorizationStatus;
        dispatch_semaphore_signal(semaphore);
    }];

    if (dispatch_semaphore_wait(semaphore, dispatch_time(DISPATCH_TIME_NOW, NOTIFICATION_TIMEOUT_SECONDS * NSEC_PER_SEC)) != 0)
        return -1;

    return status;
}

int32_t requestNotificationPermission() {
    dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
    __block int32_t granted = 0;

    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    UNAuthorizationOptions options = UNAuthorizationOptionAlert
                                   | UNAuthorizationOptionSound
                                   | UNAuthorizationOptionBadge;
    [center requestAuthorizationWithOptions:options
                          completionHandler:^(BOOL result, NSError *error) {
        if (error) {
            granted = -1;
        } else {
            granted = result ? 1 : 0;
        }
        dispatch_semaphore_signal(semaphore);
    }];

    if (dispatch_semaphore_wait(semaphore, dispatch_time(DISPATCH_TIME_NOW, NOTIFICATION_TIMEOUT_SECONDS * NSEC_PER_SEC)) != 0)
        return -1;

    return granted;
}

int32_t showNotification(char *title, char *subtitle, char *body) {
    UNMutableNotificationContent *content = [[UNMutableNotificationContent alloc] init];
    if (title)    content.title    = [NSString stringWithUTF8String:title];
    if (subtitle) content.subtitle = [NSString stringWithUTF8String:subtitle];
    if (body)     content.body     = [NSString stringWithUTF8String:body];

    NSString *identifier = [[NSUUID UUID] UUIDString];
    UNNotificationRequest *request = [UNNotificationRequest requestWithIdentifier:identifier
                                                                         content:content
                                                                         trigger:nil];

    dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
    __block int32_t result = 0;

    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    [center addNotificationRequest:request withCompletionHandler:^(NSError *error) {
        if (error) result = -1;
        dispatch_semaphore_signal(semaphore);
    }];

    if (dispatch_semaphore_wait(semaphore, dispatch_time(DISPATCH_TIME_NOW, NOTIFICATION_TIMEOUT_SECONDS * NSEC_PER_SEC)) != 0)
        return -1;

    return result;
}
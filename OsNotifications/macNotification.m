#import <UserNotifications/UserNotifications.h>

bool requestNotificationPermission() {
    dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
    __block bool granted = false;

    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    UNAuthorizationOptions options = UNAuthorizationOptionAlert
                                   | UNAuthorizationOptionSound
                                   | UNAuthorizationOptionBadge;
    [center requestAuthorizationWithOptions:options
                          completionHandler:^(BOOL result, NSError *error) {
        granted = result;
        dispatch_semaphore_signal(semaphore);
    }];

    dispatch_semaphore_wait(semaphore, DISPATCH_TIME_FOREVER);
    return granted;
}

bool isNotificationPermissionGranted() {
    dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
    __block bool granted = false;

    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    [center getNotificationSettingsWithCompletionHandler:^(UNNotificationSettings *settings) {
        granted = (settings.authorizationStatus == UNAuthorizationStatusAuthorized
                || settings.authorizationStatus == UNAuthorizationStatusProvisional
                || settings.authorizationStatus == UNAuthorizationStatusEphemeral);
        dispatch_semaphore_signal(semaphore);
    }];

    dispatch_semaphore_wait(semaphore, DISPATCH_TIME_FOREVER);
    return granted;
}

void showNotification(char *title, char *subtitle, char *body) {
    UNMutableNotificationContent *content = [[UNMutableNotificationContent alloc] init];
    if (title)    content.title    = [NSString stringWithUTF8String:title];
    if (subtitle) content.subtitle = [NSString stringWithUTF8String:subtitle];
    if (body)     content.body     = [NSString stringWithUTF8String:body];

    NSString *identifier = [[NSUUID UUID] UUIDString];
    UNNotificationRequest *request = [UNNotificationRequest requestWithIdentifier:identifier
                                                                         content:content
                                                                         trigger:nil];

    dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    [center addNotificationRequest:request withCompletionHandler:^(NSError *error) {
        dispatch_semaphore_signal(semaphore);
    }];

    dispatch_semaphore_wait(semaphore, DISPATCH_TIME_FOREVER);
}
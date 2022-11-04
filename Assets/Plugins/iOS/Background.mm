#import <UIKit/UIKit.h>
#import <CoreLocation/CoreLocation.h>

@interface Background : UIViewController <CLLocationManagerDelegate>
@end

@implementation Background

CLLocationManager *locationManager;
UIBackgroundTaskIdentifier bgTask;
NSString *prevlat = nil;
NSString *prevlong = nil;
NSDate *starttime = nil;
NSDateFormatter* df_local;
NSTimeZone *timeZone;

NSString *fname;
NSString *lname;
NSNumber *contactno;
NSString *start_time;
NSString *current_date;
NSString *filename;

-(void)startTask {
    [self endTask];
    
    prevlat = nil;
    prevlong = nil;
    starttime = nil;
    locationManager = [[CLLocationManager alloc] init];

    df_local = [[NSDateFormatter alloc] init];
    timeZone = [NSTimeZone localTimeZone];
    
    [df_local setTimeZone:timeZone];
    [df_local setDateFormat:@"MM/dd/yyyy HH:mm:ss"];

    if([locationManager respondsToSelector:@selector(requestAlwaysAuthorization)]) {
                //iOS 8.0 onwards
                [locationManager requestAlwaysAuthorization];
            }
    
    locationManager.delegate = self;
    locationManager.distanceFilter = 25;
    locationManager.allowsBackgroundLocationUpdates = YES;
    locationManager.desiredAccuracy = kCLLocationAccuracyBestForNavigation;
//    locationManager.pausesLocationUpdatesAutomatically = NO;
    [locationManager startUpdatingLocation];
}

- (void)locationManager:(CLLocationManager *)manager
     didUpdateLocations:(NSArray<CLLocation *> *)locations {
    
    if(locations.count == 0) {
            return;
        }
    NSString *locs[locations.count];
    int x = 0;
    for(CLLocation *location in locations) {
        CLLocationCoordinate2D coordinate = [location coordinate];
        NSString *latitude = [NSString stringWithFormat:@"%f", coordinate.latitude];
        NSString *longitude = [NSString stringWithFormat:@"%f", coordinate.longitude];
    //    [locationManager stopUpdatingLocation];
        NSLog(@"%@'s Location updated! x: %@   y: %@ t:%@", fname, latitude, longitude, location.timestamp);
        
        if (!prevlat || !prevlong || !starttime) {
            prevlat = latitude;
            prevlong = longitude;
            starttime = location.timestamp;
            
        }
        
        locs[x++] = [NSString stringWithFormat:@"{\"firstName\": \"%@\", \"lastName\": \"%@\", \"contactNo\": %@, \"startTime\": \"%@\", \"currentDate\": \"%@\", \"latitude\": %@, \"longitude\": %@, \"currentTime\": \"%@\"}", fname, lname, contactno, start_time, current_date, latitude, longitude, [df_local stringFromDate:location.timestamp]];
        UnitySendMessage("Main Camera", "OnDistanceChanged", [[NSString stringWithFormat:@"%@ %@ %@ %@", prevlat,latitude, prevlong, longitude] UTF8String]);
        prevlat = latitude;
        prevlong = longitude;
    }
    
    NSError *e = nil;
    NSString *contents = [NSString stringWithContentsOfFile:filename encoding:NSUTF8StringEncoding error:&e];
    
    if([contents length] > 1) contents = [NSString stringWithFormat:@"%@,\n%@", [contents substringToIndex:[contents length] - 1], locs[0]];
    else contents = [NSString stringWithFormat:@"[%@", locs[0]];
    
    for (x = 1; x < locations.count; x++) {
        contents = [NSString stringWithFormat:@"%@,\n%@", contents, locs[x]];
    }
    
    contents = [NSString stringWithFormat:@"%@]", contents];
    
    [contents writeToFile:filename atomically:YES encoding:NSUTF8StringEncoding error:nil];
    
    UnitySendMessage("Main Camera", "WriteLocationToUI", "Message to send");
    // UnitySendMessage("Main Camera", "onTimeChanged", [[NSString stringWithFormat:@"%d", -1 * (int)[starttime timeIntervalSinceNow]] UTF8String]);
}

-(void)endTask {
    if (bgTask != UIBackgroundTaskInvalid) {
        [[UIApplication sharedApplication] endBackgroundTask:bgTask];
        bgTask = UIBackgroundTaskInvalid;
    }
    [locationManager stopUpdatingLocation];
}

static Background *bg = NULL;

extern "C" {
    
    void backgroundLaunch(const char *_fname, const char *_lname, long _contactno, const char *_start_time, const char *_current_date, const char *_filename) {
        if (bg == NULL)
            bg = [[Background alloc] init];
        
        [[NSFileManager defaultManager] createFileAtPath:[NSString stringWithCString:_filename encoding:NSUTF8StringEncoding] contents:nil attributes:nil];
        NSLog(@"I hit1");
        
        fname = [NSString stringWithCString:_fname encoding:NSUTF8StringEncoding];
        NSLog(@"I hit2");
        lname = [NSString stringWithCString:_lname encoding:NSUTF8StringEncoding];
        NSLog(@"I hit3");
        contactno = [NSNumber numberWithLong:_contactno];
        NSLog(@"I hit4");
        start_time = [NSString stringWithCString:_start_time encoding:NSUTF8StringEncoding];
        NSLog(@"I hit");
        current_date = [NSString stringWithCString:_current_date encoding:NSUTF8StringEncoding];
        NSLog(@"I hit5");
        filename = [NSString stringWithCString:_filename encoding:NSASCIIStringEncoding];
        NSLog(@"I hit6");

        
        [bg startTask];
    }
    
    void backgroundStop () {
        if (bg != NULL)
            [bg endTask];
    }
}
@end

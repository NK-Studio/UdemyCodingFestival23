#import "ApplePlugin.h"

@implementation ApplePlugin

- (float)GetNativeScaleFactor {
    UIScreen *screen = [UIScreen mainScreen];
    return screen.nativeScale;
}

- (void)Vibrate
{
    AudioServicesPlaySystemSound (1520);
}

@end

extern "C" {

    void _Vibrate()
    {
        ApplePlugin* plugin = [[ApplePlugin alloc] init];
        return [plugin Vibrate];
    }

    float _GetNativeScaleFactor()
    {
            
       ApplePlugin* plugin = [[ApplePlugin alloc] init];
       return [plugin GetNativeScaleFactor];
    }
}

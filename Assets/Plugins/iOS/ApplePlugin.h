#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <AudioToolbox/AudioToolbox.h>

@interface ApplePlugin : NSObject
    - (float) GetNativeScaleFactor;
    - (void) Vibrate;
@end

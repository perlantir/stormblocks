#import <Foundation/Foundation.h>
#import <GameKit/GameKit.h>
#import <UIKit/UIKit.h>

extern UIViewController *UnityGetGLViewController(void);

@interface StormBlocksGameCenterDelegate : NSObject <GKGameCenterControllerDelegate>
@end

@implementation StormBlocksGameCenterDelegate
- (void)gameCenterViewControllerDidFinish:(GKGameCenterViewController *)gameCenterViewController
{
    [gameCenterViewController dismissViewControllerAnimated:YES completion:nil];
}
@end

static StormBlocksGameCenterDelegate *SBGameCenterDelegate(void)
{
    static StormBlocksGameCenterDelegate *delegate = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        delegate = [[StormBlocksGameCenterDelegate alloc] init];
    });
    return delegate;
}

static UIViewController *SBTopViewController(void)
{
    UIViewController *controller = UnityGetGLViewController();
    while (controller.presentedViewController != nil)
    {
        controller = controller.presentedViewController;
    }

    return controller;
}

static NSString *SBStringFromCString(const char *value)
{
    if (value == NULL)
    {
        return @"";
    }

    return [NSString stringWithUTF8String:value];
}

extern "C"
{
    void SBGameKitAuthenticate(void)
    {
        GKLocalPlayer *player = [GKLocalPlayer localPlayer];
        player.authenticateHandler = ^(UIViewController *viewController, NSError *error) {
            if (viewController != nil)
            {
                UIViewController *presenter = SBTopViewController();
                [presenter presentViewController:viewController animated:YES completion:nil];
                return;
            }

            if (error != nil)
            {
                NSLog(@"Storm Blocks Game Center authentication error: %@", error.localizedDescription);
            }
        };
    }

    bool SBGameKitIsAuthenticated(void)
    {
        return [GKLocalPlayer localPlayer].isAuthenticated;
    }

    void SBGameKitReportScore(long long score, const char *leaderboardId)
    {
        if (![GKLocalPlayer localPlayer].isAuthenticated)
        {
            return;
        }

        NSString *identifier = SBStringFromCString(leaderboardId);
        [GKLeaderboard submitScore:score
                           context:0
                            player:[GKLocalPlayer localPlayer]
                    leaderboardIDs:@[ identifier ]
                 completionHandler:^(NSError *error) {
                     if (error != nil)
                     {
                         NSLog(@"Storm Blocks Game Center score report error: %@", error.localizedDescription);
                     }
                 }];
    }

    void SBGameKitReportAchievement(const char *achievementId, double percentComplete)
    {
        if (![GKLocalPlayer localPlayer].isAuthenticated)
        {
            return;
        }

        NSString *identifier = SBStringFromCString(achievementId);
        GKAchievement *achievement = [[GKAchievement alloc] initWithIdentifier:identifier];
        achievement.percentComplete = MAX(0.0, MIN(100.0, percentComplete));
        achievement.showsCompletionBanner = achievement.percentComplete >= 100.0;
        [GKAchievement reportAchievements:@[ achievement ]
                     withCompletionHandler:^(NSError *error) {
                         if (error != nil)
                         {
                             NSLog(@"Storm Blocks Game Center achievement report error: %@", error.localizedDescription);
                         }
                     }];
    }

    void SBGameKitShowLeaderboard(void)
    {
        if (![GKLocalPlayer localPlayer].isAuthenticated)
        {
            SBGameKitAuthenticate();
            return;
        }

        GKGameCenterViewController *controller = [[GKGameCenterViewController alloc] initWithState:GKGameCenterViewControllerStateLeaderboards];
        controller.gameCenterDelegate = SBGameCenterDelegate();
        [SBTopViewController() presentViewController:controller animated:YES completion:nil];
    }

    void SBGameKitShowAchievements(void)
    {
        if (![GKLocalPlayer localPlayer].isAuthenticated)
        {
            SBGameKitAuthenticate();
            return;
        }

        GKGameCenterViewController *controller = [[GKGameCenterViewController alloc] initWithState:GKGameCenterViewControllerStateAchievements];
        controller.gameCenterDelegate = SBGameCenterDelegate();
        [SBTopViewController() presentViewController:controller animated:YES completion:nil];
    }
}

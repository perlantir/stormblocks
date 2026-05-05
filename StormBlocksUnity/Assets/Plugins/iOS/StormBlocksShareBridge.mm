#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

extern UIViewController *UnityGetGLViewController(void);

static UIViewController *SBShareTopViewController(void)
{
    UIViewController *controller = UnityGetGLViewController();
    while (controller.presentedViewController != nil)
    {
        controller = controller.presentedViewController;
    }

    return controller;
}

static NSString *SBShareStringFromCString(const char *value)
{
    if (value == NULL)
    {
        return @"";
    }

    return [NSString stringWithUTF8String:value];
}

extern "C"
{
    void SBShareTextAndImage(const char *message, const char *imagePath)
    {
        NSString *shareText = SBShareStringFromCString(message);
        NSString *path = SBShareStringFromCString(imagePath);
        dispatch_async(dispatch_get_main_queue(), ^{
            NSMutableArray *items = [NSMutableArray array];
            if (shareText.length > 0)
            {
                [items addObject:shareText];
            }

            UIImage *image = [UIImage imageWithContentsOfFile:path];
            if (image != nil)
            {
                [items addObject:image];
            }

            if (items.count == 0)
            {
                return;
            }

            UIActivityViewController *activity = [[UIActivityViewController alloc] initWithActivityItems:items applicationActivities:nil];
            UIViewController *presenter = SBShareTopViewController();
            UIPopoverPresentationController *popover = activity.popoverPresentationController;
            if (popover != nil)
            {
                popover.sourceView = presenter.view;
                popover.sourceRect = CGRectMake(CGRectGetMidX(presenter.view.bounds), CGRectGetMidY(presenter.view.bounds), 1.0, 1.0);
                popover.permittedArrowDirections = 0;
            }

            [presenter presentViewController:activity animated:YES completion:nil];
        });
    }
}

//
//  DCMainViewController.h
//  Daily Comics
//
//  Created by Johan Paul on 5/30/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import <UIKit/UIKit.h>

@class DCComicListViewController;
@class DCSettingsViewController;
@interface DCMainViewController : UIViewController {
    DCComicListViewController *comicListView;
    DCSettingsViewController *comicSettingsView;
}

@property (weak, nonatomic) IBOutlet UIButton *comcListButton;
@property (weak, nonatomic) IBOutlet UIButton *settingsButton;

- (IBAction)listTapped:(id)sender;
- (IBAction)settingsTapped:(id)sender;


@end

//
//  DCSettingsViewController.h
//  Daily Comics
//
//  Created by Johan Paul on 5/28/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import <UIKit/UIKit.h>

@class DCAppDelegate;
@class DCComicsHelper;
@interface DCSettingsViewController : UIViewController <UITableViewDelegate, UITableViewDataSource> {
    DCAppDelegate   *appDelegate;
    NSArray         *comicListCache;
    UISwitch        *selectedSwitch;
    DCComicsHelper  *comicsHelper;
    NSArray         *comicsListModel;
}

@property (weak, nonatomic) IBOutlet UITableView *settingsListView;

- (void)gotComicList: (NSData *)listJsonData;
- (void)selectedToggled:(UISwitch *)toggleSwitch;

@end

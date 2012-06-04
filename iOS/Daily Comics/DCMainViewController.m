//
//  DCMainViewController.m
//  Daily Comics
//
//  Created by Johan Paul on 5/30/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import "DCMainViewController.h"
#import "DCComicListViewController.h"

@interface DCMainViewController ()

@end

@implementation DCMainViewController
@synthesize comcListButton;
@synthesize settingsButton;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        comicListView       = [[DCComicListViewController alloc] initWithNibName:@"DCComicListViewController" 
                                                                    bundle:nil];
        self.title = @"Daily Comics";
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    // Do any additional setup after loading the view from its nib.
}

- (void)viewDidUnload
{
    [self setComcListButton:nil];
    [self setSettingsButton:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
{
    return (interfaceOrientation == UIInterfaceOrientationPortrait);
}

- (IBAction)listTapped:(id)sender {
    [self.navigationController pushViewController:comicListView animated:YES];
}

- (IBAction)settingsTapped:(id)sender {
    [self.navigationController pushViewController:comicSettingsView animated:YES];
}
@end

//
//  DCSettingsViewController.m
//  Daily Comics
//
//  Created by Johan Paul on 5/28/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//
#import <Foundation/NSJSONSerialization.h> 

#import "DCSettingsViewController.h"
#import "DCAppDelegate.h"
#import "DCComicsHelper.h"
#import "ComicStrip.h"

@interface DCSettingsViewController ()

@end

@implementation DCSettingsViewController
@synthesize settingsListView;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        self.title = @"Select comics";
        comicsHelper = [[DCComicsHelper alloc] init];
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    settingsListView.autoresizingMask = UIViewAutoresizingFlexibleHeight|UIViewAutoresizingFlexibleWidth;
    settingsListView.delegate     = self;
    settingsListView.dataSource   = self;
    
    appDelegate = (DCAppDelegate *)[[UIApplication sharedApplication] delegate];
}

- (void)viewWillAppear:(BOOL)animated 
{
    if (appDelegate.comicStripsArray.count == 0) {
        NSLog(@"No comic list found in cache. Fetching from server.");
        [comicsHelper fetchComicList:self withListSelector:@selector(gotComicList:)];
    } else {
        comicsListModel = [appDelegate comicListModel:NO];
        [settingsListView reloadData];
    }
}

- (void)gotComicList: (NSData *)listJsonData 
{
    NSLog(@"Finished loading comics list for settings page. Reloading table view...");
    
    NSError *e;
    NSDictionary *json = [NSJSONSerialization JSONObjectWithData:listJsonData options:NSJSONReadingMutableContainers error:&e];
    
    // Store the JSON list as shared data to the App delegate. The Settings view might want to re-use it.
    appDelegate.comicListJson = [json objectForKey:@"comics"];
    
    comicsListModel = [appDelegate comicListModel:NO];
    // Reload settings table now that we have the data.
    [settingsListView reloadData];
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    return [comicsListModel count];
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    static NSString *MyIdentifier = @"ComicSettingsItem";
    
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:MyIdentifier];
    
    if (cell == nil) {
        
        //NSDictionary *comicData = [appDelegate.comicListJson objectAtIndex:indexPath.row];
        ComicStrip *comic = [comicsListModel objectAtIndex:indexPath.row];
        
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:MyIdentifier];   
        cell.textLabel.text = comic.comicName;
        cell.selectionStyle = UITableViewCellSelectionStyleNone;
        
        selectedSwitch = [[UISwitch alloc] init];
        [selectedSwitch addTarget:self action:@selector(selectedToggled:) forControlEvents:UIControlEventValueChanged];
        selectedSwitch.on = [comic.comicSelected boolValue];
        
        cell.accessoryView = selectedSwitch;
    }

    return cell;
}


- (void)selectedToggled:(UISwitch *)toggleSwitch {
    BOOL selected = toggleSwitch.on;
    UITableViewCell *tableCell = (UITableViewCell *)toggleSwitch.superview;
    int rowIndex = [[settingsListView indexPathForCell:tableCell] row];
    
    NSLog(@"Switch toggled, selected %u, row %u.", selected,
                                                   rowIndex);
    
    ComicStrip *comic = [appDelegate.comicStripsArray objectAtIndex:rowIndex];
    comic.comicSelected = [NSNumber numberWithBool:selected];
    
    [appDelegate saveContext];
}

- (void)viewDidUnload
{
    [self setSettingsListView:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
{
    return (interfaceOrientation == UIInterfaceOrientationPortrait);
}

@end

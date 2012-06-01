//
//  DCMainViewController.m
//  Daily Comics
//
//  Created by Johan Paul on 5/24/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//
#import <Foundation/NSJSONSerialization.h> 

#import "DCComicListViewController.h"
#import "DCComicsHelper.h"
#import "DCComicViewController.h"
#import "DCAppDelegate.h"
#import "ComicStrip.h"

@implementation DCComicListViewController

@synthesize comicList;
@synthesize managedObjectContext = _managedObjectContext;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {        
        appDelegate = (DCAppDelegate *)[[UIApplication sharedApplication] delegate];
        
        UINavigationController *navigationController = [[UINavigationController alloc] initWithRootViewController:self];
        navigationController.navigationBar.barStyle = UIBarStyleBlack;
        
        cvc = [[DCComicViewController alloc] initWithNibName:@"DCComicViewController" 
                                                      bundle:nil];
        
        comicsHelper  = [[DCComicsHelper alloc] init];    
        
        self.title = @"Comics";
    }
    return self;
}

- (void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:animated];
    
    if (appDelegate.comicStripsArray.count == 0) {
        NSLog(@"No comic list cached. Fetching it...");
        [comicsHelper fetchComicList:self withListSelector:@selector(gotComicList:)];
    } else {
        NSLog(@"Having comic list in cache. Showing it.");
        comicsListModel = [appDelegate comicListModel:YES];
        [comicList reloadData];
    }      
}

- (void)gotComicList: (NSData *)listJsonData {
    NSString *jsonString = [[NSString alloc] initWithData:listJsonData encoding:NSUTF8StringEncoding];
    NSLog(@"Got JSON: %@", jsonString);
    
    NSError *e;
    NSDictionary *json = [NSJSONSerialization JSONObjectWithData:listJsonData options:NSJSONReadingMutableContainers error:&e];
    
    // Store the JSON list as shared data to the App delegate. The Settings view might want to re-use it.
    appDelegate.comicListJson = [json objectForKey:@"comics"];
    comicsListModel = [appDelegate comicListModel:YES];

    if (json == nil) {
        NSLog(@"Error parsing JSON!");
        return;
    }
    
    NSLog(@"Got comics, %u items.", [appDelegate.comicListJson count]);
    
    for (NSDictionary *comic in appDelegate.comicListJson) {
        NSLog(@"Comic name: %@", [comic objectForKey:@"name"]);
    }    
    
    comicList.autoresizingMask = UIViewAutoresizingFlexibleHeight|UIViewAutoresizingFlexibleWidth;
    comicList.delegate     = self;
    comicList.dataSource   = self;
    
    [comicList reloadData];
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    return [comicsListModel count];
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    static NSString *MyIdentifier = @"ComicListItem";

    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:MyIdentifier];
    
    if (cell == nil) {        
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:MyIdentifier];   
        cell.accessoryType = UITableViewCellAccessoryDetailDisclosureButton;
    }
    
    ComicStrip *comicStripData = [comicsListModel objectAtIndex:indexPath.row];
    cell.textLabel.text = comicStripData.comicName;
        
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
   // NSDictionary *comicData = [comicsData objectAtIndex:indexPath.row];
    ComicStrip *comicStripData = [comicsListModel objectAtIndex:indexPath.row];
    
    cvc.comicTag         = comicStripData.comicId;
    cvc.comicNameText    = comicStripData.comicName;
    
    NSLog(@"Selected comic name: %@, id: %@", cvc.comicNameText, cvc.comicTag);

    [self.navigationController pushViewController:cvc animated:YES];    
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
{
    return (interfaceOrientation != UIInterfaceOrientationPortraitUpsideDown);
}

- (void)viewDidUnload
{
    [self setComicList:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
}
@end

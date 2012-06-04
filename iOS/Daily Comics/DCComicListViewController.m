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
        
        [self setupFetchedResultsController];
        
        self.title = @"Comics";
    }
    return self;
}

- (void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:animated];

    if (appDelegate.comicsRefreshed == NO) {
        NSLog(@"No comic list cached. Fetching it...");
        [comicsHelper fetchComicList:self withListSelector:@selector(gotComicList:)];
    } else {
        NSLog(@"Having comic list in cache. Showing it.");
       // comicsListModel = [appDelegate comicListModel:YES];
        [comicList reloadData];
    }      
    
}

- (void)setupFetchedResultsController {
    // Setup Core data object context and request.
    managedObjectContext = appDelegate.managedObjectContext;
    NSEntityDescription *entityDescription = [NSEntityDescription entityForName:@"ComicStrip" inManagedObjectContext:managedObjectContext];
    
    NSFetchRequest *fetchRequest = [[NSFetchRequest alloc] init];
    [fetchRequest setEntity:entityDescription];
    
    // Setup the request.
    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"comicSelected == YES"];
    [fetchRequest setPredicate:predicate];
    
    NSSortDescriptor *sortDescriptor = [[NSSortDescriptor alloc] initWithKey:@"comicName" ascending:YES];
    NSArray *sortDescriptors = [[NSArray alloc] initWithObjects:sortDescriptor, nil];
    [fetchRequest setSortDescriptors:sortDescriptors];
    
    fetchResultsController = [[NSFetchedResultsController alloc]
                              initWithFetchRequest:fetchRequest 
                              managedObjectContext:managedObjectContext 
                                sectionNameKeyPath:nil 
                                         cacheName:nil];
    
    NSError *e;
    BOOL success = [fetchResultsController performFetch:&e];
    if (!success) {
        NSLog(@"Setting up NSFetchedResultsController failed!");
    }
}

- (void)gotComicList: (NSData *)listJsonData {
    NSString *jsonString = [[NSString alloc] initWithData:listJsonData encoding:NSUTF8StringEncoding];
    NSLog(@"Got JSON: %@", jsonString);
    
    NSError *e;
    NSDictionary *json = [NSJSONSerialization JSONObjectWithData:listJsonData options:NSJSONReadingMutableContainers error:&e];

    if (json == nil) {
        NSLog(@"Error parsing JSON!");
        return;
    }
    
    // Store the JSON list as shared data to the App delegate. The Settings view might want to re-use it.
    appDelegate.comicListJson = [json objectForKey:@"comics"];
    // comicsListModel = [appDelegate comicListModel:YES];
    
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
    return [[fetchResultsController sections] count];
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    id <NSFetchedResultsSectionInfo> sectionInfo = [[fetchResultsController sections] objectAtIndex:section];
    return [sectionInfo numberOfObjects];
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    static NSString *MyIdentifier = @"ComicListItem";

    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:MyIdentifier];
    
    if (cell == nil) {        
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:MyIdentifier];   
        cell.accessoryType = UITableViewCellAccessoryDetailDisclosureButton;
    }
    
    
    ComicStrip *comicStripData = [fetchResultsController objectAtIndexPath:indexPath];
    cell.textLabel.text = comicStripData.comicName;
        
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
   // NSDictionary *comicData = [comicsData objectAtIndex:indexPath.row];
    ComicStrip *comicStripData = [fetchResultsController objectAtIndexPath:indexPath];
    
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

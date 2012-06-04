//
//  DCAddComicsViewController.m
//  Daily Comics
//
//  Created by Johan Paul on 6/4/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import "DCAddComicsViewController.h"
#import "DCAppDelegate.h"
#import "ComicStrip.h"

@interface DCAddComicsViewController ()

@end

@implementation DCAddComicsViewController
@synthesize addComicsList;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        appDelegate = (DCAppDelegate *)[[UIApplication sharedApplication] delegate];
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    addComicsList.delegate   = self;
    addComicsList.dataSource = self;
    [self setupFetchedResultsController];
}

- (void)viewDidUnload
{
    [self setAddComicsList:nil];
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (void)setupFetchedResultsController {
    // Setup Core data object context and request.
    managedObjectContext = appDelegate.managedObjectContext;
    NSEntityDescription *entityDescription = [NSEntityDescription entityForName:@"ComicStrip" inManagedObjectContext:managedObjectContext];
    
    NSFetchRequest *fetchRequest = [[NSFetchRequest alloc] init];
    [fetchRequest setEntity:entityDescription];
    
    // Setup the request.
    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"comicSelected == NO"];
    [fetchRequest setPredicate:predicate];
    
    NSSortDescriptor *sortDescriptor = [[NSSortDescriptor alloc] initWithKey:@"comicName" ascending:YES];
    NSArray *sortDescriptors = [[NSArray alloc] initWithObjects:sortDescriptor, nil];
    [fetchRequest setSortDescriptors:sortDescriptors];
    
    fetchResultsController = [[NSFetchedResultsController alloc]
                              initWithFetchRequest:fetchRequest 
                              managedObjectContext:managedObjectContext 
                              sectionNameKeyPath:nil 
                              cacheName:nil];
    
    fetchResultsController.delegate = self;
    
    NSError *e;
    BOOL success = [fetchResultsController performFetch:&e];
    if (!success) {
        NSLog(@"Setting up NSFetchedResultsController failed!");
    }
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
    }
    
    ComicStrip *comicStripData = [fetchResultsController objectAtIndexPath:indexPath];
    cell.textLabel.text = comicStripData.comicName;
    
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    ComicStrip *comicStripData = [fetchResultsController objectAtIndexPath:indexPath];    
    NSLog(@"Selected comic name: %@, id: %@", comicStripData.comicName, comicStripData.comicId);
    
    comicStripData.comicSelected = [NSNumber numberWithBool:YES];
    
    [appDelegate saveContext];
    
    // If no items remain in the list, pop to the previous view automatically.
    if ([[fetchResultsController fetchedObjects] count] < 1) {
        [self.navigationController popViewControllerAnimated:YES];
    }  
}

- (void)controller:(NSFetchedResultsController *)controller didChangeObject:(id)anObject
       atIndexPath:(NSIndexPath *)indexPath forChangeType:(NSFetchedResultsChangeType)type
      newIndexPath:(NSIndexPath *)newIndexPath {
    
    NSLog(@"Table content changed.");
    
    UITableView *tableView = self.addComicsList;
    
    switch(type) {
        case NSFetchedResultsChangeDelete:
            [tableView deleteRowsAtIndexPaths:[NSArray arrayWithObject:indexPath]
                             withRowAnimation:UITableViewRowAnimationFade];
            break;
    }
}

- (void)controllerWillChangeContent:(NSFetchedResultsController *)controller {
    NSLog(@"Table content will change.");
    [self.addComicsList beginUpdates];
}

- (void)controllerDidChangeContent:(NSFetchedResultsController *)controller {
    NSLog(@"Table content did change.");
    [self.addComicsList endUpdates];
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
{
    return (interfaceOrientation == UIInterfaceOrientationPortrait);
}

@end

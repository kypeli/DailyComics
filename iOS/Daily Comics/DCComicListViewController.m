/**
 * Copyright (c) 2012, Johan Paul <johan.paul@gmail.com>
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the <organization> nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.using System;
 */

#import <Foundation/NSJSONSerialization.h> 

#import "DCComicListViewController.h"
#import "DCComicsHelper.h"
#import "DCAddComicsViewController.h"
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
        
        // Configure the NavigationController.
        //  - Title
        //  - Right will contain an edit button to remove comics from the list.
        self.navigationItem.rightBarButtonItem = self.editButtonItem;
        self.title = @"Comics";
        
        // Configure toolbar and items.
        UIBarButtonItem  *buttonItem;        
        buttonItem = [[ UIBarButtonItem alloc ] initWithTitle: @"Add removed comics"
                                                        style: UIBarButtonItemStyleBordered
                                                       target: self
                                                       action: @selector( addComic: ) ];
        self.toolbarItems = [ NSArray arrayWithObject: buttonItem ];
        
        cvc = [[DCComicViewController alloc] initWithNibName:@"DCComicViewController" 
                                                      bundle:nil];
        cvc.tweetComposeViewController = [[TWTweetComposeViewController alloc] init];
        
        comicsHelper  = [[DCComicsHelper alloc] init];    
        
        [self setupFetchedResultsController];
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
    }      
    
    comicList.autoresizingMask = UIViewAutoresizingFlexibleHeight|UIViewAutoresizingFlexibleWidth;
    comicList.delegate     = self;
    comicList.dataSource   = self;
    
    [comicList reloadData];
}

- (void)updateAddComicsButtonState {
    UIBarButtonItem *toolbarAddComicsButton = [self.toolbarItems objectAtIndex:0];
    
    if ([[fetchResultsController fetchedObjects] count] == appDelegate.totalNumberOfComics) {
        toolbarAddComicsButton.enabled = NO;
    } else {
        toolbarAddComicsButton.enabled = YES;
    }
}

- (void)viewDidAppear:(BOOL)animated {
    [self updateAddComicsButtonState];
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
    
    fetchResultsController.delegate = self;
    
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
    
    NSLog(@"Got comics, %u items.", [appDelegate.comicListJson count]);
    
    for (NSDictionary *comic in appDelegate.comicListJson) {
        NSLog(@"Comic name: %@", [comic objectForKey:@"name"]);
    }    

    
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
    ComicStrip *comicStripData = [fetchResultsController objectAtIndexPath:indexPath];
    
    cvc.comicTag         = comicStripData.comicId;
    cvc.comicNameText    = comicStripData.comicName;
    
    NSLog(@"Selected comic name: %@, id: %@", cvc.comicNameText, cvc.comicTag);

    [self.navigationController pushViewController:cvc animated:YES];    
}

- (void)tableView:(UITableView *)tableView commitEditingStyle:(UITableViewCellEditingStyle)editingStyle forRowAtIndexPath:(NSIndexPath *) indexPath
{
    
    if (editingStyle == UITableViewCellEditingStyleDelete)
    {
        ComicStrip *comicStripData = [fetchResultsController objectAtIndexPath:indexPath];
        NSLog(@"Removing row from selected contacts with tag %@", comicStripData.comicId);
        
        comicStripData.comicSelected = [NSNumber numberWithBool:NO];
    }
}


// Add comic -button handler.

- (void)addComic:(id)sender {
    NSLog(@"Add comics.");
    
    DCAddComicsViewController *addComics = [[DCAddComicsViewController alloc] initWithNibName:@"DCAddComicsViewController" 
                                                                                       bundle:nil];
    [self.navigationController pushViewController:addComics animated:YES];
}


// NSFetchedResultsControllerDelegate implementations.

- (void)controller:(NSFetchedResultsController *)controller didChangeObject:(id)anObject
       atIndexPath:(NSIndexPath *)indexPath forChangeType:(NSFetchedResultsChangeType)type
      newIndexPath:(NSIndexPath *)newIndexPath {
    
    NSLog(@"Table content changed.");
    
    UITableView *tableView = self.comicList;
    
    switch(type) {
        case NSFetchedResultsChangeDelete:
            [tableView deleteRowsAtIndexPaths:[NSArray arrayWithObject:indexPath]
                             withRowAnimation:UITableViewRowAnimationFade];
            break;
            
        case NSFetchedResultsChangeInsert:
            [tableView insertRowsAtIndexPaths:[NSArray arrayWithObject:newIndexPath]
                             withRowAnimation:UITableViewRowAnimationFade];
            break;
    }
    
    [self updateAddComicsButtonState];
    [appDelegate saveContext];
}

- (void)controllerWillChangeContent:(NSFetchedResultsController *)controller {
    NSLog(@"Table content will change.");
    [self.comicList beginUpdates];
}
    
- (void)controllerDidChangeContent:(NSFetchedResultsController *)controller {
    NSLog(@"Table content did change.");
    [self.comicList endUpdates];
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

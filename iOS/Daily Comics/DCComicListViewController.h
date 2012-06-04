//
//  DCMainViewController.h
//  Daily Comics
//
//  Created by Johan Paul on 5/24/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//
#import <CoreData/CoreData.h>

@class DCComicsHelper;
@class DCComicViewController;
@class DCAppDelegate;
@interface DCComicListViewController : UITableViewController <UITableViewDelegate, UITableViewDataSource>  {
    DCAppDelegate         *appDelegate;
    DCComicViewController *cvc;
    DCComicsHelper        *comicsHelper;
    // NSArray               *comicsListModel;
    NSManagedObjectContext *managedObjectContext;
//    NSFetchRequest        *fetchRequest;
    NSFetchedResultsController *fetchResultsController;
}

@property (strong, nonatomic) IBOutlet UITableView *comicList;
@property (strong, nonatomic) NSManagedObjectContext *managedObjectContext;

- (void)setupFetchedResultsController;
- (void)gotComicList: (NSData *)listJsonData;

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView;
- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section;
- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath;
- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath;


@end

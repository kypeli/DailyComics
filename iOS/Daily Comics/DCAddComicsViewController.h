//
//  DCAddComicsViewController.h
//  Daily Comics
//
//  Created by Johan Paul on 6/4/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import <UIKit/UIKit.h>

@class DCAppDelegate;
@interface DCAddComicsViewController : UIViewController <NSFetchedResultsControllerDelegate,
                                                        UITableViewDelegate,
                                                        UITableViewDataSource> 
{
    DCAppDelegate               *appDelegate;
    NSManagedObjectContext      *managedObjectContext;
    NSFetchedResultsController  *fetchResultsController;
}

@property (weak, nonatomic) IBOutlet UITableView *addComicsList;

- (void)setupFetchedResultsController;

@end

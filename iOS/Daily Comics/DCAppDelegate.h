//
//  DCAppDelegate.h
//  Daily Comics
//
//  Created by Johan Paul on 5/24/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import <UIKit/UIKit.h>

@class DCMainViewController;
@class ComicStrip;
@interface DCAppDelegate : UIResponder <UIApplicationDelegate> {
    UINavigationController *naviController;
    NSEntityDescription *entityDescription;
}

@property (strong, nonatomic) UIWindow *window;
@property (readonly, strong, nonatomic) NSManagedObjectContext *managedObjectContext;
@property (readonly, strong, nonatomic) NSManagedObjectModel *managedObjectModel;
@property (readonly, strong, nonatomic) NSPersistentStoreCoordinator *persistentStoreCoordinator;

@property (strong, nonatomic) NSArray        *comicListJson;
@property (strong, nonatomic) NSMutableArray *comicStripsArray;

- (BOOL)comicInCoreData:(NSString *)tag;
- (ComicStrip *)fetchComicWithTag:(NSString *)tag;
- (NSArray *)comicListModel:(BOOL)onlySelected;
- (void)saveContext;
- (NSURL *)applicationDocumentsDirectory;

@end

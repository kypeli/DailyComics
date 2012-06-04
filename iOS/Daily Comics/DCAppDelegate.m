//
//  DCAppDelegate.m
//  Daily Comics
//
//  Created by Johan Paul on 5/24/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import "DCAppDelegate.h"
#import "DCMainViewController.h"
#import "ComicStrip.h"

@implementation DCAppDelegate

@synthesize window = _window;
@synthesize managedObjectContext = __managedObjectContext;
@synthesize managedObjectModel = __managedObjectModel;
@synthesize persistentStoreCoordinator = __persistentStoreCoordinator;
@synthesize comicListJson = __comicListJson;
@synthesize comicsRefreshed;

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{        
    entityDescription = [NSEntityDescription entityForName:@"ComicStrip" inManagedObjectContext:self.managedObjectContext];
    
    DCMainViewController *mainViewController = [[DCMainViewController alloc] initWithNibName:@"DCMainViewController"
                                                                bundle:nil];

    naviController = [[UINavigationController alloc] initWithRootViewController:mainViewController];
    naviController.toolbarHidden = NO;
    
    self.comicsRefreshed = NO;

    self.window = [[UIWindow alloc] 
                   initWithFrame:[[UIScreen mainScreen] bounds]];
    
    [self.window addSubview:naviController.view];
    [self.window makeKeyAndVisible];
    
    return YES;
}

- (void)setComicListJson:(NSArray *)comicListJson {
    __comicListJson = comicListJson;
        
    for(NSDictionary *comicDict in __comicListJson) {
        NSString *comicId = [comicDict objectForKey:@"comicid"];
        
        ComicStrip *comic;
        if ([self comicInCoreData:comicId] == NO) {
            // Create and configure a new instance of the ComicStrip entity for core data.
            comic = (ComicStrip *)[NSEntityDescription insertNewObjectForEntityForName:@"ComicStrip" 
                                                                        inManagedObjectContext:self.managedObjectContext];
            
            comic.comicId   = comicId;
            comic.comicName = [comicDict objectForKey:@"name"];
            comic.comicSelected = [NSNumber numberWithBool:YES];
        
            [self saveContext];
            
            NSLog(@"Comic %@ stored to Core data.", comic.comicName);
        } else {
            comic = [self fetchComicWithTag:comicId];
        }

        self.comicsRefreshed = YES;
    }
}

- (BOOL)comicInCoreData:(NSString *)tag {
    NSFetchRequest *fetchRequest = [[NSFetchRequest alloc] init];
    [fetchRequest setEntity:entityDescription];
    
    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"comicId like[c] %@", tag];
    [fetchRequest setPredicate:predicate];
    
    NSError *error = nil;
    NSArray *result = [self.managedObjectContext executeFetchRequest:fetchRequest error:&error];

    if (result == nil || error != nil) {
        NSLog(@"Core data error: %@", error);
    }
    
    if ([result count] > 0) {
        NSLog(@"Comic with tag %@ found in core data.", tag);
        return YES;
    }
    
    NSLog(@"No comic with tag %@ found in core data.", tag);
    return NO;
}

- (ComicStrip *)fetchComicWithTag:(NSString *)tag {
    NSFetchRequest *fetchRequest = [[NSFetchRequest alloc] init];
    [fetchRequest setEntity:entityDescription]; 
    
    NSPredicate *predicate = [NSPredicate predicateWithFormat:@"comicId like[c] %@", tag];
    [fetchRequest setPredicate:predicate];
    
    NSError *error = nil;
    NSArray *results = [self.managedObjectContext executeFetchRequest:fetchRequest error:&error];
    
    if (results == nil) {
        NSLog(@"Error fetching comic with tag %@!", tag);
        return nil;
    }
    
    if([results count] != 1) {
        NSLog(@"Error fetching comic with tag %@! Got %u items, expected just one.", tag, [results count]);
        return nil;        
    }
    
    ComicStrip *strip = [results objectAtIndex:0];
    return strip;
 }

- (void)applicationWillResignActive:(UIApplication *)application
{
    // Sent when the application is about to move from active to inactive state. This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) or when the user quits the application and it begins the transition to the background state.
    // Use this method to pause ongoing tasks, disable timers, and throttle down OpenGL ES frame rates. Games should use this method to pause the game.
}

- (void)applicationDidEnterBackground:(UIApplication *)application
{
    // Use this method to release shared resources, save user data, invalidate timers, and store enough application state information to restore your application to its current state in case it is terminated later. 
    // If your application supports background execution, this method is called instead of applicationWillTerminate: when the user quits.
}

- (void)applicationWillEnterForeground:(UIApplication *)application
{
    // Called as part of the transition from the background to the inactive state; here you can undo many of the changes made on entering the background.
}

- (void)applicationDidBecomeActive:(UIApplication *)application
{
    // Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
}

- (void)applicationWillTerminate:(UIApplication *)application
{
    // Saves changes in the application's managed object context before the application terminates.
    [self saveContext];
}

- (void)saveContext
{
    NSError *error = nil;
    NSManagedObjectContext *managedObjectContext = self.managedObjectContext;
    if (managedObjectContext != nil) {
        if ([managedObjectContext hasChanges] && ![managedObjectContext save:&error]) {
             // Replace this implementation with code to handle the error appropriately.
             // abort() causes the application to generate a crash log and terminate. You should not use this function in a shipping application, although it may be useful during development. 
            NSLog(@"Unresolved error %@, %@", error, [error userInfo]);
            abort();
        } 
    }
}

#pragma mark - Core Data stack

// Returns the managed object context for the application.
// If the context doesn't already exist, it is created and bound to the persistent store coordinator for the application.
- (NSManagedObjectContext *)managedObjectContext
{
    if (__managedObjectContext != nil) {
        return __managedObjectContext;
    }
    
    NSPersistentStoreCoordinator *coordinator = [self persistentStoreCoordinator];
    if (coordinator != nil) {
        __managedObjectContext = [[NSManagedObjectContext alloc] init];
        [__managedObjectContext setPersistentStoreCoordinator:coordinator];
    }
    return __managedObjectContext;
}

// Returns the managed object model for the application.
// If the model doesn't already exist, it is created from the application's model.
- (NSManagedObjectModel *)managedObjectModel
{
    if (__managedObjectModel != nil) {
        return __managedObjectModel;
    }
    NSURL *modelURL = [[NSBundle mainBundle] URLForResource:@"Daily_Comics" withExtension:@"momd"];
    __managedObjectModel = [[NSManagedObjectModel alloc] initWithContentsOfURL:modelURL];
    return __managedObjectModel;
}

// Returns the persistent store coordinator for the application.
// If the coordinator doesn't already exist, it is created and the application's store added to it.
- (NSPersistentStoreCoordinator *)persistentStoreCoordinator
{
    if (__persistentStoreCoordinator != nil) {
        return __persistentStoreCoordinator;
    }
    
    NSURL *storeURL = [[self applicationDocumentsDirectory] URLByAppendingPathComponent:@"Daily_Comics.sqlite"];
    
    NSError *error = nil;
    __persistentStoreCoordinator = [[NSPersistentStoreCoordinator alloc] initWithManagedObjectModel:[self managedObjectModel]];
    if (![__persistentStoreCoordinator addPersistentStoreWithType:NSSQLiteStoreType configuration:nil URL:storeURL options:nil error:&error]) {
        /*
         Replace this implementation with code to handle the error appropriately.
         
         abort() causes the application to generate a crash log and terminate. You should not use this function in a shipping application, although it may be useful during development. 
         
         Typical reasons for an error here include:
         * The persistent store is not accessible;
         * The schema for the persistent store is incompatible with current managed object model.
         Check the error message to determine what the actual problem was.
         
         
         If the persistent store is not accessible, there is typically something wrong with the file path. Often, a file URL is pointing into the application's resources directory instead of a writeable directory.
         
         If you encounter schema incompatibility errors during development, you can reduce their frequency by:
         * Simply deleting the existing store:
         [[NSFileManager defaultManager] removeItemAtURL:storeURL error:nil]
         
         * Performing automatic lightweight migration by passing the following dictionary as the options parameter: 
         [NSDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithBool:YES], NSMigratePersistentStoresAutomaticallyOption, [NSNumber numberWithBool:YES], NSInferMappingModelAutomaticallyOption, nil];
         
         Lightweight migration will only work for a limited set of schema changes; consult "Core Data Model Versioning and Data Migration Programming Guide" for details.
         
         */
        NSLog(@"Unresolved error %@, %@", error, [error userInfo]);
        abort();
    }    
    
    return __persistentStoreCoordinator;
}

#pragma mark - Application's Documents directory

// Returns the URL to the application's Documents directory.
- (NSURL *)applicationDocumentsDirectory
{
    return [[[NSFileManager defaultManager] URLsForDirectory:NSDocumentDirectory inDomains:NSUserDomainMask] lastObject];
}

@end

//
//  ComicStrip.h
//  Daily Comics
//
//  Created by Johan Paul on 6/1/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CoreData/CoreData.h>


@interface ComicStrip : NSManagedObject

@property (nonatomic, retain) NSString * comicId;
@property (nonatomic, retain) NSString * comicName;
@property (nonatomic, retain) NSNumber * comicOrderNum;
@property (nonatomic, retain) NSNumber * comicSelected;

@end

//
//  DCComicsHelper.h
//  Daily Comics
//
//  Created by Johan Paul on 5/24/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import <Foundation/Foundation.h>

static NSString * const kNSStringComicListUrl       = @"http://lakka.kapsi.fi:61950/rest/comic/list";
static NSString * const kNSStringComicStripBaseUrl  = @"http://lakka.kapsi.fi:61950/rest/comic/get?id=";

@interface DCComicsHelper : NSObject {
    NSString        *urlList; 
    NSString        *urlComic;    
    NSURLRequest    *request;
    NSURLConnection *connection;
}

@property(nonatomic, retain) NSMutableData  *receivedData;
@property(nonatomic, retain) id              listDelegate;
@property(nonatomic) SEL                     listCallback;
@property(nonatomic, retain) id              comicDelegate;
@property(nonatomic) SEL                     comicCallback;

- (void)fetchComicList:(id)delegate withListSelector:(SEL)listSelector;
- (void)fetchComicInfo:(id)delegate withComicTag:(NSString *)comicTag withComicSelector:(SEL)comicSelector;
- (void)request:(NSURL *)url;


@end

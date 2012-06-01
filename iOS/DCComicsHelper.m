//
//  DCComicsHelper.m
//  Daily Comics
//
//  Created by Johan Paul on 5/24/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import "DCComicsHelper.h"

@implementation DCComicsHelper

@synthesize listCallback, listDelegate, comicCallback, comicDelegate;
@synthesize receivedData;

- (id) init {
    self = [super init];
    urlList = kNSStringComicListUrl;
    
    return self;
}

- (void)fetchComicList:(id)delegate withListSelector:(SEL)listSelector {
    NSLog(@"Fetching comic list.");
    self.listDelegate = delegate;
    self.listCallback = listSelector;
    
    NSURL *listUrl = [NSURL URLWithString:urlList];
    [self request:listUrl];
}

- (void) fetchComicInfo:(id)delegate withComicTag:(NSString *)comicTag withComicSelector:(SEL)comicSelector {
    NSLog(@"Fetching comic info for comic tag: %@", comicTag);
    self.comicCallback = comicSelector;
    self.comicDelegate = delegate;
  
    urlComic = [NSString stringWithFormat:
                @"%@%@", kNSStringComicStripBaseUrl, comicTag];

    
    NSURL *comicInfoUrl = [NSURL URLWithString:urlComic];
 
    [self request:comicInfoUrl];
}

- (void)request:(NSURL *)url {
    NSLog(@"Web request: %@", url);
    
    request     = [[NSMutableURLRequest alloc] initWithURL:url];
    connection  = [[NSURLConnection alloc] initWithRequest:request delegate:self];
    
    if (connection) {
        receivedData = [NSMutableData alloc];
    }
}

- (void)connection:(NSURLConnection *)connection didReceiveData:(NSData *)data {
    [receivedData appendData:data];
}

- (void)connectionDidFinishLoading:(NSURLConnection *)connection {
    NSLog(@"HTTP request finished.");
    
   // NSString *data = [[NSString alloc] initWithData:receivedData encoding:NSUTF8StringEncoding];
    
    if (self.listDelegate && self.listCallback) {
        if ([self.listDelegate respondsToSelector:self.listCallback]) {
            [self.listDelegate performSelector:self.listCallback withObject:receivedData];
            self.listCallback = nil;
            self.listDelegate = nil;
        } else {
            NSLog(@"No delegate for comic list found.");
        }
    } 

    if (self.comicDelegate && self.comicCallback) {
        if ([self.comicDelegate respondsToSelector:self.comicCallback]) {
            [self.comicDelegate performSelector:self.comicCallback withObject:receivedData];
            self.comicCallback = nil;
            self.comicDelegate = nil;
        } else {
            NSLog(@"No delegate for comic data found.");
        }
    } 

}


@end

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

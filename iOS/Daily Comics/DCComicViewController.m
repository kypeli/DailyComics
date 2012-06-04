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

#import "DCComicViewController.h"
#import "DCComicsHelper.h"

#import <Foundation/NSJSONSerialization.h> 

@interface DCComicViewController ()

@end

@implementation DCComicViewController
@synthesize scrollView;
@synthesize comicPubDateUILabel;
@synthesize activityIndicator;
@synthesize comicView;
@synthesize comicTag;
@synthesize comicNameText;
@synthesize comicPubDateText;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        comicsHelper    = [[DCComicsHelper alloc] init];
        comicImageCache = [[NSMutableDictionary alloc] init]; 
    }
    return self;
}

- (void)viewWillAppear:(BOOL)animated
{
    [super viewWillAppear:animated];
    self.title                      = self.comicNameText;
    self.comicPubDateUILabel.text   = self.comicPubDateText;
        
    // Check if comic image has been cached. If not, fetch comic data from the server.
    if ([comicImageCache objectForKey:comicTag] == nil) {
        NSLog(@"Getting comic data from the web.");
        
        [comicsHelper fetchComicInfo:self 
                        withComicTag:self.comicTag 
                   withComicSelector:@selector(gotComicData:)];
    } else {
        NSLog(@"Showing comic from cache.");
        [self showComicFromCache:comicTag];
    }

}

- (void)gotComicData: (NSData *)comicJsonData {
//    NSString *json = [[NSString alloc] initWithData:comicJsonData encoding:NSUTF8StringEncoding];
    NSLog(@"Comic JSON loaded.");
    
    // Fetch JSON from the reply.
    NSError *e;
    NSDictionary *comicJson = [NSJSONSerialization JSONObjectWithData:comicJsonData options:NSJSONReadingMutableContainers error:&e];
    NSURL *comicUrl         = [NSURL URLWithString:[comicJson objectForKey:@"url"]];
    self.comicPubDateUILabel.text = [comicJson objectForKey:@"pubdate"];
    
    // Fetch image data.
    if(httpConn) {
        [httpConn cancel];
    }
    
    [self.activityIndicator startAnimating];
    
    NSURLRequest *request = [NSURLRequest requestWithURL:comicUrl
                                             cachePolicy:NSURLRequestReturnCacheDataElseLoad
                                         timeoutInterval:30];
    
    // Hide any previously cached version.
    self.comicView.hidden = YES;
    
    imageData = [NSMutableData new];
    httpConn  = [NSURLConnection connectionWithRequest:request 
                                              delegate:self];
    
    NSLog(@"Started loading comic strip from web, URL: %@", comicUrl);  
    
}
    
/*    NSData  *comicImageData = [NSData dataWithContentsOfURL:comicUrl];
    UIImage *comicImage = [UIImage imageWithData:comicImageData];
    
    CGRect screenRect = [[UIScreen mainScreen] bounds];
    CGFloat screenWidth = screenRect.size.width;
    CGFloat screenHeight = screenRect.size.height;
    
    CGRect frame = self.comicView.frame;
    frame.size.width = screenWidth;
    self.comicView.frame = frame;
    self.comicView.contentMode = UIViewContentModeScaleAspectFit;
    
    [self.comicView setImage:comicImage];
*/
/*    CGSize comicSize = CGSizeMake(screenWidth, comicImage.size.height);
    self.scrollView.contentSize =  comicSize;
}
 */

- (void)showComicFromCache: (NSString *)comicId {
    UIImage *comicImage = [comicImageCache objectForKey:comicId];
    [self.comicView setImage:comicImage];
}

- (void)connection:(NSURLConnection *)connection didReceiveData:(NSData *)data {
    [imageData appendData:data];
}

- (void)connectionDidFinishLoading:(NSURLConnection *)connection {
    NSLog(@"Finished loading comic image from web, showing...");

    UIImage *comicImage = [UIImage imageWithData:imageData];
    
    // Store image to cache
    [comicImageCache setValue:comicImage forKey:comicTag];
    
    // Show image on the UI and stop loading indicator.
    [self.comicView setImage:comicImage];
    self.comicView.hidden = NO;

    [self.activityIndicator stopAnimating];
    
    imageData = nil;
    httpConn = nil;
}

- (void)viewDidUnload
{
    [self setComicView:nil];
    [self setScrollView:nil];
    [self setComicPubDateUILabel:nil];
    [self setActivityIndicator:nil];
    [super viewDidUnload];
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
{
    return (interfaceOrientation == UIInterfaceOrientationPortrait);
}

@end

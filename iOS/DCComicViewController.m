//
//  DCComicViewController.m
//  Daily Comics
//
//  Created by Johan Paul on 5/24/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//
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

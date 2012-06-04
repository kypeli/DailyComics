//
//  DCComicViewController.h
//  Daily Comics
//
//  Created by Johan Paul on 5/24/12.
//  Copyright (c) 2012 __MyCompanyName__. All rights reserved.
//

#import <UIKit/UIKit.h>

@class DCComicsHelper;
@interface DCComicViewController : UIViewController {
    DCComicsHelper      *comicsHelper;
    NSURLConnection     *httpConn;
    NSMutableData       *imageData;
    NSMutableDictionary *comicImageCache;
}

@property (weak, nonatomic) IBOutlet UIImageView *comicView;
@property (weak, nonatomic) IBOutlet UIScrollView *scrollView;
@property (weak, nonatomic) IBOutlet UILabel *comicPubDateUILabel;
@property (weak, nonatomic) IBOutlet UIActivityIndicatorView *activityIndicator;

@property (weak, nonatomic) NSString *comicTag;
@property (weak, nonatomic) NSString *comicNameText;
@property (weak, nonatomic) NSString *comicPubDateText;

- (void)gotComicData: (NSData *)comicJsonData;
- (void)showComicFromCache: (NSString *)comicId;

@end

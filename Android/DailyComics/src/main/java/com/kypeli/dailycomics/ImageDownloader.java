package com.kypeli.dailycomics;

import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.drawable.BitmapDrawable;
import android.os.AsyncTask;
import android.util.Log;
import android.view.View;
import android.widget.ImageView;

import java.io.InputStream;

/**
 * Created by user on 28/12/13.
 */
public class ImageDownloader extends AsyncTask<String, Void, Bitmap> {
    private final String TAG = "DC:ImageLoader";
    View bmImage;

    public ImageDownloader(View bmImage) {
        this.bmImage = bmImage;
    }

    protected Bitmap doInBackground(String... urls) {
        String urldisplay = urls[0];
        Bitmap mIcon11 = null;

        Log.d(TAG, "Fetching image from URL: " + urldisplay);

        try {
            InputStream in = new java.net.URL(urldisplay).openStream();
            mIcon11 = BitmapFactory.decodeStream(in);
        } catch (Exception e) {
            Log.e("Error", e.getMessage());
            e.printStackTrace();
        }
        return mIcon11;
    }

    protected void onPostExecute(Bitmap result) {
        bmImage.setBackground(new BitmapDrawable(Resources.getSystem(), result));
    }
}

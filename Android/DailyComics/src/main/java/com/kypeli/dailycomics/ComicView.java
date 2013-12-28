package com.kypeli.dailycomics;

import android.content.Context;
import android.graphics.drawable.Drawable;
import android.util.AttributeSet;
import android.view.View;

/**
 * Created by user on 28/12/13.
 */
public class ComicView extends View {
    public ComicView(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
    }

    public ComicView(Context context, AttributeSet attrs) {
        super(context, attrs);
    }

    public ComicView(Context context) {
        super(context);
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        Drawable bg = getBackground();
        if (bg != null
            && bg.getIntrinsicHeight() > 0
            && bg.getIntrinsicWidth() > 0) {
            int width = MeasureSpec.getSize(widthMeasureSpec);
            int height = width * bg.getIntrinsicHeight() / bg.getIntrinsicWidth();
            setMeasuredDimension(width,height);
        }
        else {
            super.onMeasure(widthMeasureSpec, heightMeasureSpec);
        }
    }
}

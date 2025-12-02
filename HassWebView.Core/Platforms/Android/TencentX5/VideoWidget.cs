using Android.Graphics;
using Android.Media;
using Android.Text;
using Android.Util;
using Android.Views;
using Com.Tencent.Smtt.Sdk;

namespace HassWebView.Core.Platforms.Android.TencentX5;

public class VideoWidget : Java.Lang.Object, IEmbeddedWidgetClient
{
    private const string TAG = "VideoWidget";

    private readonly string _tagName;
    private readonly IDictionary<string, string> _attributes;
    private readonly IEmbeddedWidget _widget;

    private MediaPlayer _player;
    private string _videoSrc;

    public VideoWidget(string tagName,
                       IDictionary<string, string> attributes,
                       IEmbeddedWidget widget)
    {
        _tagName = tagName;
        _attributes = attributes;
        _widget = widget;

        if (_attributes.ContainsKey("src"))
        {
            _videoSrc = _attributes["src"];
        }
    }

    public void OnSurfaceCreated(Surface surface)
    {
        Log.Info(TAG, "OnSurfaceCreated: " + surface);

        if (_player == null)
        {
            _player = new MediaPlayer();

            if (!string.IsNullOrEmpty(_videoSrc))
            {
                Play();
            }
        }

        _player.SetSurface(surface);
    }

    public void OnSurfaceDestroyed(Surface surface)
    {
        Log.Info(TAG, "OnSurfaceDestroyed: " + surface);
    }

    public bool OnTouchEvent(MotionEvent e)
    {
        if (_videoSrc == null) return false;

        if (e.Action == MotionEventActions.Up)
        {
            if (_player.IsPlaying)
                _player.Pause();
            else
                _player.Start();
        }
        return true;
    }

    public void OnRectChanged(Rect rect)
    {
        Log.Info(TAG, "OnRectChanged: " + rect);
    }

    public void OnDestroy()
    {
        Log.Info(TAG, "OnDestroy");

        if (_player != null)
        {
            _player.Stop();
            _player.Release();
            _player = null;
        }
    }

    public void OnActive()
    {
        if (_player != null && !_player.IsPlaying)
            _player.Start();
    }

    public void OnDeactive()
    {
        if (_player != null && _player.IsPlaying)
            _player.Pause();
    }

    public void OnRequestRedraw()
    {
    }

    public void OnVisibilityChanged(bool visibility)
    {
        if (_player == null) return;

        if (visibility)
            _player.Start();
        else
            _player.Pause();
    }

    private void Play()
    {
        try
        {
            _player.SetDataSource(_videoSrc);
            _player.Looping = true;
            _player.PrepareAsync();
            _player.Prepared += (s, e) =>
            {
                Log.Info(TAG, "onPrepared");
                _player.Start();
            };
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex.ToString());
        }
    }
}

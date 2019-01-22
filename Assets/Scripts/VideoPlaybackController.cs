using UnityEngine;
using System.Collections;
using Vuforia;


public class VideoPlaybackController : MonoBehaviour
{
	#region PRIVATE_MEMBER_VARIABLES

	private Vector2 mTouchStartPos;
	private bool mTouchMoved = false;
	private float mTimeElapsed = 0.0f;

	private bool mTapped = false;
	private float mTimeElapsedSinceTap = 0.0f;

	private bool mWentToFullScreen = false;

	#endregion



	#region UNITY_MONOBEHAVIOUR_METHODS

	void Update()
	{

		CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
		if (Input.touchCount > 0)
		{
			Touch touch = Input.touches[0];
			if (touch.phase == TouchPhase.Began)
			{
				mTouchStartPos = touch.position;
				mTouchMoved = false;
				mTimeElapsed = 0.0f;
			}
			else
			{
				mTimeElapsed += Time.deltaTime;
			}

			if (touch.phase == TouchPhase.Moved)
			{
				if (Vector2.Distance(mTouchStartPos, touch.position) > 40)
				{
					mTouchMoved = true;
				}
			}
			else if (touch.phase == TouchPhase.Ended)
			{
				if (!mTouchMoved && mTimeElapsed < 1.0)
				{
					if (mTapped)
					{
						HandleDoubleTap();
						mTapped = false;
					}
					else
					{
						mTapped = true;
						mTimeElapsedSinceTap = 0.0f;
					}
				}
			}
		}

		if (mTapped)
		{
			if (mTimeElapsedSinceTap >= 0.5f)
			{
				OnSingleTapConfirmed();
				HandleTap();
				mTapped = false;
			}
			else
			{
				mTimeElapsedSinceTap += Time.deltaTime;
			}
		}
		if (Input.GetKeyUp(KeyCode.Escape))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Vuforia-1-About");
        }
	}

	#endregion 



	#region PRIVATE_METHODS

	private void HandleTap()
	{
		VideoPlaybackBehaviour video = PickVideo(mTouchStartPos);

		if (video != null)
		{
			if (video.VideoPlayer.IsPlayableOnTexture())
			{

				VideoPlayerHelper.MediaState state = video.VideoPlayer.GetStatus();
				if (state == VideoPlayerHelper.MediaState.PAUSED ||
					state == VideoPlayerHelper.MediaState.READY ||
					state == VideoPlayerHelper.MediaState.STOPPED)
				{
					PauseOtherVideos(video);

					video.VideoPlayer.Play(false, video.VideoPlayer.GetCurrentPosition());
				}
				else if (state == VideoPlayerHelper.MediaState.REACHED_END)
				{
					PauseOtherVideos(video);

					video.VideoPlayer.Play(false, 0);
				}
				else if (state == VideoPlayerHelper.MediaState.PLAYING)
				{
					video.VideoPlayer.Pause();
				}
			}
			else
			{
				video.ShowBusyIcon();

				video.VideoPlayer.Play(true, 0);
				mWentToFullScreen = true;
			}
		}
			
	}


	private void HandleDoubleTap()
	{
		VideoPlaybackBehaviour video = PickVideo(mTouchStartPos);

		if (video != null)
		{
			if (video.VideoPlayer.IsPlayableFullscreen())
			{
				video.VideoPlayer.Pause();

				video.VideoPlayer.SeekTo(0.0f);

				video.ShowBusyIcon();

				video.VideoPlayer.Play(true, 0);
				mWentToFullScreen = true;
			}
		}
	}

	private VideoPlaybackBehaviour PickVideo(Vector3 screenPoint)
	{
		VideoPlaybackBehaviour[] videos = (VideoPlaybackBehaviour[])
			FindObjectsOfType(typeof(VideoPlaybackBehaviour));

		Ray ray = Camera.main.ScreenPointToRay(screenPoint);
		RaycastHit hit = new RaycastHit();

		foreach (VideoPlaybackBehaviour video in videos)
		{
			if (video.GetComponent<Collider>().Raycast(ray, out hit, 10000))
			{
				return video;
			}
		}

		return null;
	}


	private void PauseOtherVideos(VideoPlaybackBehaviour currentVideo)
	{
		VideoPlaybackBehaviour[] videos = (VideoPlaybackBehaviour[])
			FindObjectsOfType(typeof(VideoPlaybackBehaviour));

		foreach (VideoPlaybackBehaviour video in videos)
		{
			if (video != currentVideo)
			{
				if (video.CurrentState == VideoPlayerHelper.MediaState.PLAYING)
				{
					video.VideoPlayer.Pause();
				}
			}
		}
	}

	#endregion 

	protected virtual void OnSingleTapConfirmed()
    {
        
    }

	#region PUBLIC_METHODS

	public bool CheckWentToFullScreen()
	{
		bool result = mWentToFullScreen;
		mWentToFullScreen = false;
		return result;
	}

	#endregion 
}
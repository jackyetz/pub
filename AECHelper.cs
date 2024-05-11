using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Dissonance;
using NAudio.Wave;
using Crosstales.Common.Model.Enum;

namespace AECHelper
{
    public class AECHelper : MonoBehaviour
    {
        #region 变量
        public const int samplerate = 48000;
        public const int loopLength = 5;    // 一次取5secs
        DissonanceComms comms;
        public AudioSource audiosrc;
        public AudioClip audioclip;
        MyMicrophoneSubscriber MicS;
        #endregion
        public string gameobjname = "";
        public bool playlocalspeech = true;
        public AECHelper(string gameobjn="", bool playlocals=true)
        {
            gameobjname = gameobjn;
            playlocalspeech = playlocals;
        }
        // Start is called before the first frame update
        public void Start()
        {
            MicS = gameObject.AddComponent<MyMicrophoneSubscriber>();
            comms = GameObject.Find(gameobjname).GetComponent<DissonanceComms>();
            audiosrc = GameObject.Find(gameobjname).GetComponent<AudioSource>();

            comms.SubscribeToRecordedAudio(MicS);
            StartCoroutine(WaitRemoteClip());
        }
        IEnumerator WaitRemoteClip()
        {
            audioclip = AudioClip.Create("MySinusoid", samplerate*loopLength, 1, samplerate, true, MicS.OnAudioRead, MicS.OnAudioSetPosition);
            audiosrc.clip = audioclip;
            //audiosrc.loop = true;
            if(playlocalspeech)
                audiosrc.Play();
            yield return null;
        }
        public void Update()
        {
            MicS.Update();
        }
    }

    public class MyMicrophoneSubscriber : BaseMicrophoneSubscriber
    {
        List<float> bufferlist = new List<float>();
        int minlength;

        protected override void ProcessAudio(ArraySegment<float> data)
        {
            bufferlist.AddRange(data.ToArray());
        }

        protected override void ResetAudioStream(WaveFormat waveFormat)
        {
            if (waveFormat.SampleRate != AECHelper.samplerate)
                throw new NotImplementedException("Wrong sample rate");
            bufferlist.Clear();
        }
        public override void Update()
        {
            base.Update();
        }
        //After you have copied some amount of data from the bufferlist into the data array,
        // you need to remove exactly that much data from the start of the buffer.
        public void OnAudioRead(float[] data)
        {
            //Debug.Log("OnAudioRead: " + bufferlist.Count.ToString() + ", " + data.Length.ToString());
            minlength = (bufferlist.Count < data.Length) ? bufferlist.Count : data.Length;
            if (minlength > 0)
            {
                bufferlist.CopyTo(0, data, 0, minlength);
                bufferlist.RemoveRange(0, minlength);
            }
            //else
            //    Array.Clear(data, 0, data.Length);
        }

        public void OnAudioSetPosition(int newPosition)
        {
            Debug.Log("OnAudioSetPosition: " + newPosition.ToString());
        }
    }
}

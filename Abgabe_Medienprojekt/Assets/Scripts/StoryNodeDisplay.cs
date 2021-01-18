using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StoryNodeDisplay : MonoBehaviour
{
    public StoryNode beginning;

    public StoryNode node;

    public Stack<StoryNode> nodestack = new Stack<StoryNode>();

    public VideoPlayer vp1;

    public VideoPlayer vp2;

    private VideoPlayer activeVideoPlayer;

    private VideoPlayer bufferingVideoPlayer;

    public GameObject[] buttons;

    public GameObject Restart;

    public GameObject Tryagain;

    public GameObject Quit;

    private readonly int[] indices = new int[3];

    private int currentVideoIndex;

    private bool currentNodeFinished;

    private bool buttonsAreShown = false;

    //Player soll nicht beim Start abspielen, Events an das Enden eines Videos bei vp1 und vp2 gesetzt und Methode StartPlayCurrentNode aufgerufen
    private void Start()
    {
        this.vp2.playOnAwake = false;
        this.vp1.playOnAwake = false;

        this.vp1.loopPointReached += this.OnVideoPlayerFinished;
        this.vp2.loopPointReached += this.OnVideoPlayerFinished;

        this.StartPlayCurrentNode();
    }

    private void StartPlayCurrentNode()
    {
        //Listener abgemeldet
        this.Tryagain.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        this.Restart.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        this.Quit.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        // Videoplayer gestoppt
        this.vp1?.Stop();
        this.vp2?.Stop();
        //Button ausgeblendet
        this.Restart.SetActive(false);
        this.Tryagain.SetActive(false);
        this.Quit.SetActive(false);
        //Knoten auf Stack gelegt
        this.nodestack.Push(this.node);
        //aktueller Knoten auf nicht beendet gelegt
        this.currentNodeFinished = false;
        // damit bei PlayNextVideo mit 0 angefangen wird
        this.currentVideoIndex = -1;
        //vp1 zum Buffer
        this.bufferingVideoPlayer = this.vp1;
        this.PrepareNextVideo();
        this.PlayNextVideo();
    }

    //Spielt den aktuellen Knoten beim letzten Video ab
    private void StartPlayCurrentNodeLastVideo(int index)
    {
        //Listener abgemeldet
        this.Tryagain.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        this.Restart.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        this.Quit.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        // Videoplayer gestoppt
        this.vp1?.Stop();
        this.vp2?.Stop();
        //Button ausgeblendet
        this.Restart.SetActive(false);
        this.Tryagain.SetActive(false);
        this.Quit.SetActive(false);
        //Knoten auf Stack gelegt
        this.nodestack.Push(this.node);
        //aktueller Knoten auf nicht beendet gelegt
        this.currentNodeFinished = false;
        //damit letztes Video abgespielt wird
        this.currentVideoIndex = index;
        //vp1 zum Buffer
        this.bufferingVideoPlayer = this.vp1;
        this.PrepareNextVideo();
        this.PlayNextVideo();
    }

    //Bereitet das nächste Video im bufferVideoplayer vor
    private void PrepareNextVideo()
    {
        this.bufferingVideoPlayer.clip = this.node.mainvideos[this.currentVideoIndex + 1];
        this.bufferingVideoPlayer.EnableAudioTrack(0, true);
    }

    private void PlayNextVideo()
    {
        //wenn aktiverplayer gesetzt dann hänge Event OnActiveVideoPlayerFrameReady ab
        if (this.activeVideoPlayer != null)
        {
            this.activeVideoPlayer.frameReady -= this.OnActiveVideoPlayerFrameReady;
        }

        //der Aktivevideoplayer bekommt Inhalt des Buffervideoplayer
        this.activeVideoPlayer = this.bufferingVideoPlayer;
        //zum schnelleren abspielen
        //this.activeVideoPlayer.frame = (long)(this.activeVideoPlayer.frameCount * 0.8);
        
        //Bereiten das Standbild des letzten Frames vor
        this.activeVideoPlayer.sendFrameReadyEvents = true;
        this.activeVideoPlayer.frameReady += this.OnActiveVideoPlayerFrameReady;

        this.currentVideoIndex++;
        
        //wenn vp1 der aktive ist setze vp2 in buffer, //wenn vp2 der aktive ist setze vp1 in buffer
        this.bufferingVideoPlayer = this.activeVideoPlayer == this.vp1 ? this.vp2 : this.vp1;

        //gibt es noch weiteres video
        if (this.currentVideoIndex + 1 < this.node.mainvideos.Count)
        {
            this.PrepareNextVideo();
        }
        else
        {
            // wir haben alle Videos gespielt und setzten Boolean finished true
            this.currentNodeFinished = true;
        }
        this.activeVideoPlayer.Play();
    }

    //freeze the last Frame
    private void OnActiveVideoPlayerFrameReady(VideoPlayer source, long frameIdx)
    {
        //nur wenn alle Videos aus Knoten gespielt und Buttons nicht angezeigt werden und wenn der aktuelle frameIdx innerhalb der letzten 3 Frames vom Video ist
        if (this.currentNodeFinished && !this.buttonsAreShown && ((long)source.frameCount - 3 < frameIdx))
        {
            //Pausiere Video => Standbild
            source.Pause();
            //hängen Event wieder ab
            source.frameReady -= this.OnActiveVideoPlayerFrameReady;
            //Zeige die Buttons mit den Antworten an
            this.ShowDecisionsInButtons();
        }
    }


    private void OnVideoPlayerFinished(VideoPlayer source)
    {
        if (this.currentNodeFinished && !this.buttonsAreShown)
        {
            //Sicherheitsmasnahme falls freeze last Frame nicht funktioniert
            this.ShowDecisionsInButtons();
        }
        else
        {
            //Spiele nächstes Video
            this.PlayNextVideo();
        }
    }

    //Zeigt die Buttons und deren ANtworten an
    private void ShowDecisionsInButtons()
    {
        //Alle Antworten als key-value-paar in decisions abspeichern
        var decisions = this.node.GetDecisions();
        var buttonIndex = 0;
        foreach (var item in decisions)
        {            
            //Falls key ein leerer String dann gibt es keine weiteren Knoten
            if (item.Key == "")
            {
                //WENN KEINE WEITEREN DECISIONS DANN RESTART Quit UND TRYAGAIN ANZEIGEN
                this.Restart.SetActive(true);
                this.Tryagain.SetActive(true);
                this.Quit.SetActive(true);
                //Buttons bekommen Listener mit Methoden falls ausgelöst
                this.Restart.gameObject.GetComponent<Button>().onClick.AddListener(() => this.spawnFromBeginning());
                this.Tryagain.gameObject.GetComponent<Button>().onClick.AddListener(() => this.spawnFromLastDecision());
                this.Quit.gameObject.GetComponent<Button>().onClick.AddListener(() => this.closeApplication());
                return;
            }

            //Setzt den richtigen Text im Button[i] 
            this.buttons[buttonIndex].GetComponentInChildren<Text>().text = item.Key;
            this.indices[buttonIndex] = item.Value;

            //zeigt den Button an
            this.buttons[buttonIndex].gameObject.SetActive(true);
            var idx = buttonIndex;
            // Hängt einen Listner mit Methode an
            this.buttons[buttonIndex].gameObject.GetComponent<Button>().onClick.AddListener(() => this.OnClick(idx));

            buttonIndex++;
        }
        //boolean das die Buttons angezeigt werden wird auf true gesetzt
        this.buttonsAreShown = true;
    }
    
    //Es wird zur letzten Entscheidung gespawnt
    public void spawnFromLastDecision()
    {
        //es werden zwei Knoten vom Stack genommen und der oberste dann dem Aktuellen Node übergeben und abgespielt
        this.nodestack.Pop();
        this.node = this.nodestack.Pop();
        int index = node.mainvideos.Count - 2;
        this.StartPlayCurrentNodeLastVideo(index);
    }

    //der aktuelle Knoten bekommt die Informationen vom Beginning Knoten und wird abgespielt
    public void spawnFromBeginning()
    {
        this.node = this.beginning;
        this.StartPlayCurrentNode();
    }

    //schließt Application
    public void closeApplication()
    {
        print("Schließe Applikation");
        Application.Quit();
    }

    //Setzt den geklickten Knoten auf den Aktuellen Knoten
    private void OnClick(int Idx)
    {
        for (var i = 0; i < 3; i++)
        {
            //Listener werden abgehängt von den Buttons und inaktiv gesetzt
            this.buttons[i].gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            this.buttons[i].gameObject.SetActive(false);
        }
        //Boolean buttonsareShown korrigiert
        this.buttonsAreShown = false;
        //der aktivePlayer wird gestoppt
        this.activeVideoPlayer.Stop();
        //Es wird der geklickte Button und damit der passende Knoten zum aktuellen gestzt und abgespielt
        this.node = this.node.NextNode(this.indices[Idx]);
        this.StartPlayCurrentNode();
    }
}
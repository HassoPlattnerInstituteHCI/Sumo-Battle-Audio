//==========================================
// Title:  DialogGraph.cs
// Author: Jotaro Shigeyama, Corinna Jascheck and Thijs Roumen (firstname.lastname [at] hpi.de)
// Date:   2020.04.20
//==========================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public delegate Task DialogAction(object param);

public class DialogNode
{
    private List<DialogOption> options;
    private string sentences;
    private DialogAction action;
    private object actionarg;
    private AudioSource soundSource;
    private SpeechIn asr;
    private SpeechOut tts;
    public DialogNode(string sentences, DialogAction callback = null, object param = null)    {
        this.sentences = sentences;
        options = new List<DialogOption>();
        action = callback;
        actionarg = param;
    }
    public DialogNode(AudioSource source,AudioClip playSound, DialogAction callback = null, object param = null)
    {
        sentences = "";
        soundSource = source;
        soundSource.clip = playSound;
        options = new List<DialogOption>();
        action = callback;
        actionarg = param;
    }
    internal void addOption(DialogNode node) {
        options.Add(new DialogOption(node));
        }
    internal void addOption(string command, DialogNode node)
    {
        List<string> commands = new List<string>();
        commands.Add(command);
        options.Add(new DialogOption(node, commands));
    }
    internal void addOption(List<string> commands, DialogNode node)
    {
        options.Add(new DialogOption(node, commands));
    }
    internal void addOptions(   List<string> commands1 = null, DialogNode node1 = null,
                                List<string> commands2 = null, DialogNode node2 = null,
                                List<string> commands3 = null, DialogNode node3 = null)
    {
        addOption(commands1, node1);
        if (commands2 != null) 
            addOption(commands2, node2);
        if (commands3 != null)
            addOption(commands3, node3);
    }
    internal void addOptions(   string command1 = null, DialogNode node1 = null,
                                string command2 = null, DialogNode node2 = null,
                                string command3 = null, DialogNode node3 = null)
    {
        addOption(command1, node1);
        if (command2 != null)
            addOption(command2, node2);
        if (command3 != null)
            addOption(command3, node3);
    }
    internal async Task playSound() {
        soundSource.Play();
        while (soundSource.isPlaying)
            await Task.Delay(100);
        return;
    }
    public async void play(SpeechIn speechIn, SpeechOut speechOut, bool silent = false)
    {
        asr = speechIn;
        tts = speechOut;
        if (!silent & sentences != "") await tts.Speak(sentences);
        if (soundSource && soundSource.clip)
            await playSound();
        if (action != null) await action.Invoke(actionarg);
        string recognized;
        switch (options.Count)
        {
            case 0: //no options, endNode
                return;
            case 1: //single option, listen without return
                DialogOption singleOption = options.ToArray()[0];
                if (singleOption.commands == null)  //option has no commands, move on to next node
                {
                    singleOption.next.play(asr, tts);
                    return;
                }
                recognized = await asr.Listen(singleOption.commands.ToArray());
                if (checkMetaCommands(recognized) == false)
                    singleOption.next.play(asr, tts);
                break;
            default: // various options
                recognized = await asr.Listen(generateCommandArray());
                foreach (DialogOption option in options)
                    if (option.commands.Contains(recognized))
                        option.next.play(asr, tts);
                checkMetaCommands(recognized);
                return;
        }
    }
    internal string[] generateCommandArray()
    {
        List<string> commandList = new List<string>();
        foreach (DialogOption option in options)
            commandList.AddRange(option.commands);
        return commandList.ToArray();
    }
    internal bool checkMetaCommands(string input)
    {
        if (asr.getMetaCommands().Contains(input))
        {
            play(asr, tts, true);
            return true;
        }
        return false;
    }
}
public class DialogOption
{
    public List<string> commands;
    public DialogNode next;
    public DialogOption(DialogNode next, List<string> commands = null)
    {
        this.commands = commands;
        this.next = next;
    }
}
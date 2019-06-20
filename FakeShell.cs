/*
  If, for some reason, you want to create a fake unix shell, this is that.
  Plug this into your video game by feeding text input into HandleInput()
  and then appending the console output to that same text box.

  Notice that piping between programs isn't implemented and 
  all these programs are cardboard cutouts of the real thing.

  This should be relatively easy to extend if you want to add a new
  program, though a captive user interface would be pretty challenging
  if you intend this to work outside of the terminal/commandline.
*/
using System;
using System.Collections.Generic;

public class FakeShell {
  public bool running;
  
  // File structure:
  // [0] leading path
  // [1] file name
  // [2] data
  public List<string[]> fakeFiles;
  public string workingDirectory, user, computer;

  public List<string> stdout, stdin;
  public int outputMode; // 1 = console, 2 = pipe

  public FakeShell(){
    computer = "badguyterminal";
    user = "elitehaxxorprotagonist";

    outputMode = 1;
    Output("Last login: Tue Jan 12 22:29:30 2079");
    running = true;
    fakeFiles = new List<string[]>{
      new string[]{"","/", "DIR"},
      new string[]{"/","home", "DIR"},
      new string[]{"/","usr", "DIR"},
      new string[]{"/usr/","secretformula.txt", "It's plankton!"}
    };
    workingDirectory = FullFilePath(fakeFiles[0]);
  }

  public void HandleInput(string inputLine){
    string[] commands = inputLine.Split(new Char [] {'>' , '|' });
    if(commands == null || commands.Length == 0){
      Console.Write("Not sure how, but 0 commands were parsed.");
    }
    if(commands.Length == 1){
      ExecuteCommand(inputLine);
      return;
    }

    outputMode = 2;

    Console.Write("Congratulations! You used a pipe or carrot.\n");
    
    outputMode = 1; 
  }

  public void ExecuteCommand(string inputLine){
    string[] args = inputLine.Split(' ');
    if(outputMode == 1 && QuotesParse(inputLine) != ""){
      args[1] = QuotesParse(inputLine);
    }
    if(outputMode == 2 && stdout != null && stdout.Count > 0){
      List<string> tmp = new List<string>();
      tmp.AddRange(args);
      tmp.AddRange(stdout);
      stdout = new List<string>();
      args = tmp.ToArray();
    }
    if(args.Length == 0 || args[0] == ""){
      return;
    }
    
    switch(args[0].ToLower()){
      case "clear":
        CLEAR(args);
      break;
      case "ls": 
        LS(args);
      break;
      case "cd":
        CD(args);
        break;
      case "exit":
        Exit(args);
      break;
      case "pwd":
        PWD(args);
      break;
      case "rm":
        RM(args);
      break;
      case "touch":
        TOUCH(args);
      break;
      case "mkdir":
        MKDIR(args);
      break;
      case "cat":
        CAT(args);
      break;
      case "echo":
        ECHO(args);
      break;
      default:
        Output(args[0] + ": command not found");
      break;
    }
  }

  //################################################
  //      Fake Programs
  //################################################

  public void CLEAR(string[] args){
    Output("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
  }
  public void PWD(string[] args){
    Output(workingDirectory);
  }

  public void LS(string[] args){
    string directory = workingDirectory;

    if(args.Length > 1 && FileIndex(EvaluatePath(args[1])) == -1){
      Output("ls: cannot access '" + args[1] + "': No such file or directory");
      return;
    }
    else if(args.Length > 1){
      directory = FullFilePath(fakeFiles[FileIndex(EvaluatePath(args[1]))]);
    }

    List<string[]> matchingFiles = FilesInDir(directory);
    int i = 0;
    string output = "";
    foreach(string[] file in matchingFiles){
      output += FileName(file) + "\t";
      i++;
      if(i == 3){
        i = 0;
        Output(output);
        output = "";
      }
    }
    if(output != ""){
      Output(output);
    }
  }

  public void CD(string[] args){
    if(args.Length == 1 || args[1].Equals("./")){
      return;
    }

    if(args[1].Equals("../") || args[1].Equals("..")){
      workingDirectory = OneDirectoryUp(workingDirectory);
      return;
    }

    int fileIndex = FileIndex(EvaluatePath(args[1]));
    if(fileIndex == -1){
      Output("cd: '" + args[1] + "': No such file or directory");
      return;
    }
    workingDirectory = FullFilePath(fakeFiles[fileIndex]);
  }

  public void RM(string[] args){
    if(args == null || args.Length < 2){
      Output("rm: missing operand");
      return;
    }
    int fileIndex = FileIndex(args[1]);

    if(fileIndex == -1){
      Output("rm: cannot remove '" + args[1] + "': No such file or directory");
      return;
    }

    fakeFiles.RemoveAt(fileIndex);
  }

  public void MKDIR(string[] args){
    string path = EvaluatePath(args[1]);
    string dir = OneDirectoryUp(path);
    string name = FileNameFromPath(path);
    string data = "DIR";

    if(FileIndex(dir) == -1){
      Output("mkdir: cannot create directory ‘" + args[1] + "’: No such file or directory");
      return;
    }
    
    if(FileIndex(path, true) != -1){
      Output("mkdir: cannot create directory ‘" + args[1] + "’: File exists");
      return;
    }

    fakeFiles.Add(new string[]{ dir, name, data });
  }

  public void TOUCH(string[] args){
    if(args.Length == 1){
      return;
    }
    string path = EvaluatePath(args[1]);
    string dir = OneDirectoryUp(path);
    string name = FileNameFromPath(path);
    string data = "";

    if(FileIndex(path) != -1){
      return;
    }
    fakeFiles.Add(new string[]{ dir, name, data });
  }

  public void CAT(string[] args){
    if(args.Length == 1){
      return;
    }
    string path = EvaluatePath(args[1]);
    int fileIndex = FileIndex(path);
    if(fileIndex == -1){
      Output("cat: '" + args[1] + "': No such file or directory");
      return;
    }
    string[] file = fakeFiles[fileIndex];
    if(file[2].Equals("DIR")){
      Output("cat: '" + file[1] + "': Is a directory");
      return;
    }
    Output(file[2]);
  }

  public void ECHO(string[] args){
    string output = "";
    
    if(args.Length > 1){
      output = args[1];
    }

    Output(output);
  }

  public void Exit(string[] args){
    running = false;
  }

  //################################################
  //      Change these if you want to swap out
  //      console i/o for something else
  //################################################

  public static void Main(string[] args){
    FakeShell shell = new FakeShell();
    while(shell.Running()){
      shell.HandleInput(Console.ReadLine());
    }
  }

  public void PrintPrompt(){
    Console.Write(user + "@" + computer + ":" + workingDirectory + "$  ");
  }

  public void Output(string message){
    if(outputMode == 1){
      Console.Write(message + "\n"); 
    }
    else if(outputMode == 2){
      STDOUT(message);
    }
  }

  //################################################
  //      Helper methods
  //################################################

  public string QuotesParse(string fullLine){
    string[] quotesSplit = fullLine.Split('"');
    if(quotesSplit.Length > 2){
      return quotesSplit[1];
    }
    return "";
  }

  public List<string[]> FilesInDir(string path){
    List<string[]> ret = new List<string[]>();
    foreach(string[] file in fakeFiles){
      if(file[0].Equals(path)){
        ret.Add(file);
      }
    }
    return ret;
  }

  public string FullFilePath(string[] file){
    if(file[0] == ""){
      return file[0] + file[1];
    }
    return file[0] + file[1] + "/";
  }

  public string FileNameFromPath(string path){
    if(path.Equals("/")){
      return path;
    }
    if(path[path.Length-1] == '/'){
      path = path.Substring(0, path.Length -1);
    }
    
    int slashIndex = path.LastIndexOf('/');
    if(slashIndex == -1){
      return path;
    }

    return path.Substring(path.LastIndexOf('/') + 1, path.Length - (path.LastIndexOf('/') + 1));
  }

  public string FileName(string[] file){
    if(file[2].Equals("DIR")){
      return file[1] + "/";
    }
    return file[1];
  }

  public int FileIndex(string path, bool mustBeDirectory = false){
    string pathAsDir = path;
    if(path[path.Length -1] != '/'){
      pathAsDir += "/";
    }
    for(int i = 0; i < fakeFiles.Count; i++){
      string fakePath = FullFilePath(fakeFiles[i]);
      if(fakePath.Equals(path) && !mustBeDirectory || fakePath.Equals(pathAsDir)){
        return i;
      }
    }
    return -1;
  }

  public string OneDirectoryUp(string path){
    if(path.Equals("/") || path.Equals("") || path.Equals(" ")){
      return path;
    }
    if(path.Split('/').Length - 1 < 2){
      return "/";
    }
    if(path[path.Length-1] == '/'){
      path = path.Substring(0, path.Length -1);
    }

    path = path.Substring(0, path.LastIndexOf('/') + 1);
    return path;
  }

  // Populate STDOUT buffer
  public void STDOUT(string message){
    if(stdout == null){
      stdout = new List<string>();
    }
    stdout.Add(message);
  }

  public bool Running(){
    if(!running){
      return false;
    }
    PrintPrompt();
    return true;
  }

  public string EvaluatePath(string path){
    if(path.Equals("")){
      return "";
    }
    if(path[0] == '/'){
      return path;
    }
    return workingDirectory + path;
  }
}

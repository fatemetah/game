import java.net.*;
import java.util.ArrayList;
import java.io.*;
// Java program to create a text File using FileWriter
import java.util.*;

public class Server
{

    public void createDataBase(){
        try {
            File myObj = new File("db.txt");
            if (myObj.createNewFile()) {
                System.out.println("File created: " + myObj.getName());
            } else {
                System.out.println("File already exists.");
            }
        } catch (Exception e) {
        }
    };
    public void addNewScore(Score score){
        String filename= "db.txt";
        try{
            FileWriter fw = new FileWriter(filename,true); //the true will append the new data
            fw.write(Integer.toString(score.score) + "," + score.name + "\n" );//appends the string to the file
            fw.close();
            Score.score_board.add(score);
        }catch(Exception e){}
    }
    public void loadScores(){
        Score.score_board.clear();
        File file = new File("db.txt");
        try(BufferedReader br = new BufferedReader(new FileReader(file))) {

            for(String line; (line = br.readLine()) != null; ) {
                // System.out.println(line.split(",")[1]);
                Score new_score = new Score(line.split(",")[1], Integer.parseInt(line.split(",")[0]));
                Score.score_board.add(new_score);
            }
            // line is not visible here.
        }catch(Exception e){
            System.out.println(e.getMessage());
        }
        Collections.sort(Score.score_board);
    }
    public void getTopTen(){
        this.loadScores();

    }



    public Server(int port)
    {
        try
        {
            createDataBase();
            ServerSocket server_socket = new ServerSocket(port);

            // System.out.println("Server started");
 
            // System.out.println("Waiting for a client ...");

            Socket client_socket = server_socket.accept();
 
            // System.out.println("Client accepted");
 
            // takes input from the client socket
            DataInputStream in = new DataInputStream(new BufferedInputStream(client_socket.getInputStream()));
            DataOutputStream out=new DataOutputStream(client_socket.getOutputStream()); 
   
            String line = "";
 
            while (true)
            {
                try
                {
                    line = in.readUTF();
                    if(line.equals("Close"))break;
                    ClientCommand cmd = new ClientCommand(line);

                    if(cmd.args.isEmpty()){
                        out.writeUTF("not enough argument");
                        continue;
                    }
                    switch(cmd.command_name){
                        case "getTopTen":
                            out.writeUTF("");
                            out.flush();
                            break;
                        case "getHighest":
                            out.writeUTF(Score.score_board.get(0).name +","+ Integer.toString(Score.score_board.get(0).score));
                            out.flush();
                            break;
                        case "setNewScore":
                            addNewScore(new Score( cmd.args.get(0), Integer.parseInt(cmd.args.get(1)) ));
                            out.writeUTF("done!");
                            out.flush();
                            break;
                        default:
                            out.writeUTF("not enough argument");
                    }
 
                }
                catch(Exception i)
                {
                }
            }
            
            // close connection
            server_socket.close();
            in.close();
        }
        catch(IOException i)
        {
        }
    }




public static void main(String[] args)
    {
        Server server = new Server(1000);

        // ClientCommand cm = new ClientCommand("pow(1,2)");
        // System.out.println(cm.args);
    }

}

class ClientCommand{
    String command_name = "";
    ArrayList<String> args = new ArrayList<String>();
    public ClientCommand(String cmd){
        if(!cmd.contains("(")) return;
        this.command_name = cmd.split("\\(")[0];

        if(cmd.split("\\(")[1].split("\\)").length >= 1){
            for(String arg :cmd.split("\\(")[1].split("\\)")[0].split(",")){
                args.add(arg);
            }
        }
    }
}

class Score implements Comparable<Score>{
    static ArrayList<Score> score_board = new ArrayList<Score>();
    String name = "";
    int score;
    public Score(String name, int score){
        this.score = score;
        this.name = name;
    }

    @Override
    public int compareTo(Score other) {
        return this.score - other.score; // or whatever property you want to sort
    }
}
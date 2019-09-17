package client;

/**
 * Created by Bassem on 4/15/2015.
 */
public class ReportException extends Exception {
    public ReportException(){
        super();
    }

    public ReportException(String msg){
        super(msg);
    }
	
    public ReportException(String msg, Throwable cause) {
        super(msg, cause);
    }
}

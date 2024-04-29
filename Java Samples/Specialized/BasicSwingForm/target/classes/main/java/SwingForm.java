import javax.swing.*;
import java.awt.*;

/**
 *this class is used for design of the basic swing example
 */
public class SwingForm {
    public JPanel root;
    public JButton XML;
    public JButton MySql;
    public JProgressBar progressBar;
    public JLabel Status;
    public JButton DB2;
    public JButton MSSQL;

    public SwingForm() {
        root = new JPanel();
        root.setLayout(new GridLayout(3, 2, -1, -1));

        XML = new JButton();
        XML.setText("Run XML Report");

        MySql = new JButton();
        MySql.setText("Run MySql Report");

        progressBar = new JProgressBar();

        Status = new JLabel();

        DB2 = new JButton();
        DB2.setText("Run DB2 Report");

        MSSQL = new JButton();
        MSSQL.setText("Run MS SQL Report");

        root.add(XML);
        root.add(MySql);
        root.add(MSSQL);
        root.add(DB2);

        root.add(progressBar);
        root.add(Status);
    }

}
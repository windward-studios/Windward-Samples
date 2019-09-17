package client;

/**
 * Created by Bassem on 4/16/2015.
 */
public class Version {
    public String serviceVersion,engineVersion;

    public Version(String serviceVersion,String engineVersion){
        this.engineVersion = engineVersion;
        this.serviceVersion = serviceVersion;
    }
}

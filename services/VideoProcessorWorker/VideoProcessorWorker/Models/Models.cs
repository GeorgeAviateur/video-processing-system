public class S3Event
{
    public List<S3Record> Records { get; set; }
}

public class S3Record
{
    public S3Entity s3 { get; set; }
}

public class S3Entity
{
    public S3Bucket bucket { get; set; }
    public S3Object @object { get; set; }
}

public class S3Bucket
{
    public string name { get; set; }
}

public class S3Object
{
    public string key { get; set; }
}
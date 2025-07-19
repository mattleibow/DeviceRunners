namespace DeviceRunners.UIAutomation;

public interface IInMemoryFile
{
	Stream ToStream();

	byte[] ToByteArray();

	string ToBase64String();
}

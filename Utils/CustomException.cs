namespace TraineeManagementApi.Utils.CustomException;

public class NotFoundException(string message) : Exception(message);

public class UnauthorizedException(string message) : Exception(message);

public class BadRequestException(string message) : Exception(message);

public class FileNotFound() : Exception();

public class JwtOperationException() : Exception();

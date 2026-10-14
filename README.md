# DocxPreviewApi — .doc/.docx to PDF Preview Service

English | [繁體中文](README.zh-TW.md)

A small **.NET 10 (ASP.NET Core Minimal API)** module that converts `.doc` and `.docx` files to PDF and displays them in a web page, using **LibreOffice** as the conversion engine. Serves the frontend at `http://localhost:7090/` and runs in a **Docker Compose** container.

## Why this project

Many web apps need to preview Word documents without asking the user to download them. This module shows a minimal, working approach: the server converts the document to PDF on demand and the browser displays it inline.

## Tech Stack

| Category | Technology |
|---|---|
| Web framework | ASP.NET Core 10 (Minimal API) |
| Frontend | Single HTML/JS page served from `wwwroot/` |
| Document conversion | LibreOffice (`soffice`, headless) |
| Fonts | Noto CJK, Liberation (for CJK and general text rendering) |
| Containerization | Docker, Docker Compose |
| Port | 7090 |

## Features

- **Single endpoint, `POST /api/convert`**: accepts a `.doc` or `.docx` file, returns a PDF.
- **Frontend preview**: the demo page (served from `wwwroot/`) posts the file and displays the returned PDF in an `<iframe>`.
- **Per-request isolated profile directory**: each conversion runs in its own temporary directory with its own LibreOffice user profile, preventing concurrent requests from locking each other out.
- **Timeout and error handling**: a conversion that runs longer than 30 seconds is terminated. Errors from LibreOffice are returned in the response.
- **Automatic cleanup**: temporary files and the LibreOffice profile directory are deleted after each request.

## API

| Method | Path | Description |
|---|---|---|
| POST | `/api/convert` | Upload a `.doc` or `.docx` file, receive a PDF |

Returns:
- `200` with the PDF file (as `application/pdf`) on success.
- `400` if no file is uploaded or the file is not `.doc` or `.docx`.
- `500` (ProblemDetails) if conversion fails or times out.

## Getting Started

1. Build and start the container:

```bash
docker compose up --build
```

2. Open `http://localhost:7090/`.

3. Select a `.doc` or `.docx` file. The page converts it and displays the resulting PDF inline.

## How it works

1. The frontend posts the file as `multipart/form-data` to `POST /api/convert`.
2. The server writes the uploaded file into a new temporary directory, unique to this request.
3. LibreOffice is started in headless mode with its own user profile in that same directory, and converts the file to PDF.
4. The server returns the PDF bytes to the browser, which displays them in an `<iframe>`.

## Known limitations

- Conversion is CPU-bound and synchronous; a very large file can tie up a request slot until the 30-second timeout.
- There is no authentication or rate limiting. It's designed as a small module to be embedded behind another API, not exposed publicly.

## License

MIT

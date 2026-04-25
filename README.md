# 🎬 Video Processing System (AWS + .NET)

A distributed video processing pipeline built with **.NET**, **AWS S3**, **SQS**, and **FFmpeg**.

This project demonstrates how to design a scalable, event-driven system where video uploads trigger asynchronous processing to generate thumbnails.

---

## 🚀 Features

* Upload videos using **pre-signed URLs**
* Automatic event-driven processing via **AWS S3 → SQS**
* Background worker built with **.NET Hosted Services**
* Thumbnail generation using **FFmpeg**
* Scalable architecture (ready for multiple workers)

---

## 🏗️ Architecture

```text
Client → API (Presigned URL)
        ↓
     AWS S3 (uploads/)
        ↓ (event notification)
     AWS SQS
        ↓
   .NET Worker
        ↓
  Thumbnails → S3 (thumbnails/)
```

---

## 📸 Demo

### Upload flow

![Upload Demo](./docs/upload.gif)

### Processing flow

![Processing Demo](./docs/processing.gif)


---

## ⚙️ Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/)
* AWS Account
* Configured:

  * S3 bucket
  * SQS queue
  * IAM permissions
* FFmpeg installed locally

```bash
brew install ffmpeg
```

---

## 🔐 Environment Variables

Set the following:

```bash
export AWS_ACCESS_KEY=your_key
export AWS_SECRET_KEY=your_secret
export AWS_REGION=us-east-2
export AWS_BUCKET_NAME=your_bucket
export AWS_SQS_QUEUE_URL=your_queue_url
```

---

## ▶️ Running the Project

### 1. Start the API (for presigned URLs)

```bash
dotnet run
```

### 2. Start the worker

```bash
cd VideoProcessorWorker
dotnet run
```

---

## 📂 Project Structure

```text
services/
 ├── Api/
 └── VideoProcessorWorker/
```

---

## 🧠 How It Works

1. Client requests a **pre-signed URL**
2. Video is uploaded directly to **S3**
3. S3 emits an event to **SQS**
4. Worker consumes the message
5. Video is downloaded and processed
6. Thumbnails are generated and uploaded to `thumbnails/`

---

## ⚠️ Notes

* S3 event notifications must be configured correctly
* SQS must allow `s3.amazonaws.com` to send messages
* Prefix filters (e.g. `uploads/`) must match upload keys

---

## 🤖 About AI Assistance

This project was developed with the support of an AI coding assistant.

The assistant was used to:

* accelerate development
* clarify architectural decisions
* debug integration issues

All implementation, understanding, and final decisions were made by the developer.

---

## 📈 Future Improvements

* Add video transcoding (HLS streaming)
* Introduce a database for metadata
* Implement retry & dead-letter queues
* Split into multiple specialised workers
* Deploy using Docker & CI/CD

---

## 👨‍💻 Author

Jorge Garcia
Software Engineer (.NET)

---

## ⭐ Why this project?

This project was built to practice **real-world backend architecture**, focusing on:

* distributed systems
* event-driven design
* cloud integration (AWS)

---

## 📜 License

MIT

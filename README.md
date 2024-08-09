# Image Dimension Checker

This project processes image URLs listed in an Excel file, checks the dimensions of each image, and outputs the results to a CSV file. Invalid image URLs or images that cannot be processed are logged with their corresponding IDs in a separate CSV file.

## Features

- Reads image URLs from an Excel file.
- Downloads images using the provided URLs.
- Retrieves the width and height of each image.
- Logs valid image dimensions (ID, Width, Height) into a CSV file.
- Logs invalid image IDs into a separate CSV file.

## Prerequisites

- .NET 6.0 SDK
- Excel file containing image URLs (with IDs)

## How to Use

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/abdurrahmanyesilyurt/Image_Process.git
   cd Image_Process

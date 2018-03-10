using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace scoring_client
{
	public class ImageUtils
	{

		public static int[][][] ConvertImageStreamToDimArrays(Bitmap bitmap)
		{
			var bitmapArray = BitmapToByteArray(bitmap);
			using (var memoryStream = new MemoryStream(bitmapArray))
			{
				memoryStream.Position = 0;
				return ConvertImageDataToDimArrays(bitmap.Height, bitmap.Width, 3, memoryStream);
			}
		}

		public static int[][][] ConvertImageStreamToDimArrays(Stream stream)
		{
			using (var bitmap = new Bitmap(stream))
			{
				var bitmapArray = BitmapToByteArray(bitmap);
				using (var memoryStream = new MemoryStream(bitmapArray))
				{
					memoryStream.Position = 0;
					return ConvertImageDataToDimArrays(bitmap.Height, bitmap.Width, 3, memoryStream);
				}
			}
		}

		//convert image to byte array
		private static byte[] BitmapToByteArray(Bitmap img)      //img is the input image. Image width and height in pixels. PixelFormat is 24 bit per pixel in this case
        {
            BitmapData bmpData=img.LockBits(new Rectangle(0,0,img.Width,img.Height),ImageLockMode.ReadOnly,img.PixelFormat);    //define and lock the area of the picture with rectangle
            int pixelbytes =Image.GetPixelFormatSize(img.PixelFormat) / 8;     //for 24 bpp the value of pixelbytes is 3, for 32 bpp is 4, for 8 bpp is 1
            var ptr=bmpData.Scan0;      //this is a memory address, where the bitmap starts
            var psize = bmpData.Stride * bmpData.Height;      // picture size in bytes
            byte[] byOut=new byte[psize];     //create the output byte array, which size is obviously the same as the input one
            System.Runtime.InteropServices.Marshal.Copy(ptr, byOut, 0, psize);      //this is a very fast method, which copies the memory content to byteOut array, but implemented for 24 bpp pictures only
            img.UnlockBits(bmpData);      //release the locked memory
            return byOut;      
        }

		private static int[][][] ConvertImageDataToDimArrays(int numRows, int numCols, int numChannels, MemoryStream stream)
		{
			var imageMatrix = new int[numRows][][];
			for (int row = 0; row < numRows; row++)
			{
				imageMatrix[row] = new int[numCols][];
				for (int col = 0; col < numCols; col++)
				{
					imageMatrix[row][col] = new int[numChannels];
					for (int channel = 0; channel < numChannels; channel++)
					{
						imageMatrix[row][col][channel] = stream.ReadByte();
					}
				}
			}
			return imageMatrix;
		}




		public static Bitmap ConvertDimArraysToImageBitmap(int[][][] dimArray, int length, int width, int height)
		{
			var byteOut = ConvertDimArraysToImageData(dimArray, length, width, height);
			return ByteArrayToBitmap(byteOut, width, height);
		}

		private static byte[] ConvertDimArraysToImageData(int[][][] dimArray, int length, int width, int heigth)
		{
			var byteOut = new byte[length];
			var t = 0;
			for (int row = 0; row < width; row++)
			{
				for (int col = 0; col < heigth; col++)
				{
					for (int channel = 0; channel < dimArray[row][col].GetUpperBound(0)+1; channel++)
					{
						byteOut[t] = (byte)(dimArray[row][col][channel]);
						t++;
					}
				}
			}
			return byteOut;
		}

		 //convert byte array to bitmap
        private static Bitmap ByteArrayToBitmap(byte[] byteIn, int imwidth, int imheight)     // byteIn the input byte array. Picture size should be known
        {
            Bitmap picOut=new Bitmap(imwidth,imheight,PixelFormat.Format24bppRgb);  //define the output picture
            BitmapData bmpData = picOut.LockBits(new Rectangle(0, 0, imwidth, imheight), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            var ptr=bmpData.Scan0;
            var psize =   bmpData.Stride*imheight;
            System.Runtime.InteropServices.Marshal.Copy(byteIn,0,ptr,psize);
            picOut.UnlockBits(bmpData);
            return picOut;
        }
	}
}
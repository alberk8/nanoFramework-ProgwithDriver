using System;
using System.Collections;
using System.Device.Gpio;
using System.Device.Spi;
using System.Text;
using System.Threading;

namespace Drivers
    {
        public class NHD420cwSingleton
        {
            //Singleton BoardFlash
            //assures a single instance of BoardFlash.cs
            //static bool init = false;
            //static int instanceCounter = 0;
            private static NHD420cwSingleton singleInstance = null;

            private NHD420cwSpi _Driver = null;

            private static object lockThis = new object();

            private NHD420cwSingleton()
            {
                _Driver = new NHD420cwSpi();//SpiModule = SPI.SPI_module.SPI3,

                _Driver.init_oled();
            }

            public static NHD420cwSpi _oledDisplay
            {
                get
                {
                    lock (lockThis)
                    {
                        if (singleInstance == null)
                        {
                            singleInstance = new NHD420cwSingleton();
                        }
                        return singleInstance._Driver;
                    }
                }
            }

            public partial class NHD420cwSpi
            {


                private SpiConnectionSettings _spiConfig;                // SPI configuration

                private SpiDevice _spi;

                private static GpioController OutputPort;

                private int _resetPin;




                private static int t = 0;



                public NHD420cwSpi()
                {
                    {
                        //_spiConfig = new SPI.Configuration(
                        //    Cs,
                        //    false,
                        //    15,
                        //    10,
                        //    true,
                        //    true,
                        //    20000,
                        //    spiModule);

                        if (_spiConfig == null)
                        {
                            _spiConfig = new SpiConnectionSettings(
                                2,  //	BusId = other.BusId;  SPI1,SPI2, etc
                                32);     // pin to use as chip select

                            _spiConfig.ClockFrequency = 20000;
                            _spiConfig.Mode = SpiMode.Mode3;
                            _spiConfig.ChipSelectLineActiveState = false;

                        }

                        if (_spi == null)
                        {
                            _spi = SpiDevice.Create(_spiConfig);
                        }

                        if (OutputPort == null)
                        {
                            OutputPort = new GpioController();

                        }

                        _resetPin = 26 ; //Gpio.IO26;//OK
                        OutputPort.OpenPin(_resetPin, PinMode.Output);
                        //OutputPort.Write(_resetPin, PinValue.Low);
                    }
                }

                /// <summary>
                /// The Newhaven serial LCD displays all support one of three possible
                /// communication schemes. So that the driver is independent of the 
                /// communication schemes, it depends on the user to supply the communication
                /// function for talking to the display. This function has been 
                /// Normalize to the least common function. In this case a function 
                /// that takes a parameter of as a byte array, and returns nothing.
                /// 
                /// It is actually appropriate that the LCD driver be only concerned 
                /// with the commands to be sent to the display and not how they get
                /// there.
                /// </summary>
                /// <param name="msg"></param>
                public delegate void sendDelegate(byte[] msg);

                /// <summary>
                /// The constructor will fill in the value for this delegate function to 
                /// be used by the driver.
                /// </summary>
                //private sendDelegate SendToInterface;

                //private const byte OLED_COMMANDMODE = 0x80;
                //private const byte OLED_DATAMODE = 0x40;
                private const byte OLED_SETBRIGHTNESSCOMMAND = 0x81;
                // commands
                private const byte LCD_CLEARDISPLAY = 0x01;
                private const byte LCD_RETURNHOME = 0x02;
                private const byte LCD_ENTRYMODESET = 0x04;
                private const byte LCD_DISPLAYCONTROL = 0x08;
                private const byte LCD_CURSORSHIFT = 0x10;
                //private const byte LCD_FUNCTIONSET = 0x28;
                private const byte LCD_SETCGRAMADDR = 0x40;
                private const byte LCD_SETDDRAMADDR = 0x80;

                // flags for display entry mode
                //private const byte LCD_ENTRYRIGHT = 0x00;
                private const byte LCD_ENTRYLEFT = 0x02;
                private const byte LCD_ENTRYSHIFTINCREMENT = 0x01;
                //private const byte LCD_ENTRYSHIFTDECREMENT = 0x00;

                // flags for display on/off control
                private const byte LCD_DISPLAYON = 0x04;
                private const byte LCD_DISPLAYOFF = 0x00;
                private const byte LCD_CURSORON = 0x02;
                private const byte LCD_CURSOROFF = 0x00;
                private const byte LCD_BLINKON = 0x01;
                private const byte LCD_BLINKOFF = 0x00;

                // flags for display/cursor shift
                private const byte LCD_DISPLAYMOVE = 0x08;
                //private const byte LCD_CURSORMOVE = 0x00;
                private const byte LCD_MOVERIGHT = 0x04;
                private const byte LCD_MOVELEFT = 0x00;

                // flags for function set
                //private const byte LCD_8BITMODE = 0x10;
                //private const byte LCD_4BITMODE = 0x00;
                //private const byte LCD_ENGLISH_JAPANESE = 0x00;
                //private const byte LCD_WESTERN_EUROPEAN_1 = 0x01;
                //private const byte LCD_ENGLISH_RUSSIAN = 0x02;
                //private const byte LCD_WESTERN_EUROPEAN_2 = 0x03;

                //uint _displayfunction;
                private byte _displaycontrol;
                private byte _displaymode;
                //private byte _initialized;
                //private byte _currline;
                private byte _numlines = (byte)4;
                private byte _numcols = (byte)20;

                //uint oldValueBar1 = 0;

                //byte blockSimbol = 0x1F;
                //byte voidSimbol = 0x0F;
                //byte minusSimbol = 0x2D;
                //byte pointSimbol = 0xDD;
                //byte arrowSimbol = 0xDF;
                //byte d_arrowSimbol = 0x19;
                //byte f_arrowSimbol = 0x10;
                //byte slashSimbol = 0xC4;

                //byte vBar1Simbol = 0xDA;
                //byte vBar2Simbol = 0xD9;
                //byte vBar3Simbol = 0xD8;
                //byte vBar4Simbol = 0xD7;

                uint currentColLine0 = 0;
                uint currentColLine1 = 0;
                uint currentColLine2 = 0;
                uint currentColLine3 = 0;

                //byte[] row_offsets = { 0x80, 0xA0, 0xC0, 0xE0 };
                //byte CODEPAGE = (byte)CodePage.ROM_B;
                //byte CODEPAGE = (byte)CodePage.ROM_A;
                public int DISPLAY_LINES = 4;
                public int DISPLAY_CHARS = 20;

                private byte[] Line_Adresses = { 0x80, 0xA0, 0xC0, 0xE0 }; // RAM addresses for line 0-3

                //public enum CodePage : byte
                //{
                //    ROM_A = 0x02,
                //    ROM_B = 0x06,
                //    ROM_C = 0x0A
                //}

                /// <summary>
                /// Send the supplied byte array to the LCD device interface.
                /// </summary>
                /// <param name="SendData"> The array of bytes to be sent.</param>
                private void SendToInterface(byte[] data)
                {
                    //write_buffer = data;

                    _spi.Write(new SpanByte(data));
                    //Hardware.SPIBus.Write(write_buffer);
                }

                public uint reverse_byte(uint a)
                {
                    uint b = ((a & 0x1) << 7) | ((a & 0x2) << 5) |
                             ((a & 0x4) << 3) | ((a & 0x8) << 1) |
                             ((a & 0x10) >> 1) | ((a & 0x20) >> 3) |
                             ((a & 0x40) >> 5) | ((a & 0x80) >> 7);

                    return b;
                }

                public void SendCommand(byte c)
                {
                    byte temp = 0xF8;
                    byte[] send = new byte[3];
                    send[0] = temp;
                    send[2] = (byte)(c & 0xF0);
                    send[1] = (byte)(c << 4);

                    send[2] = (byte)reverse_byte(send[2]);
                    send[1] = (byte)reverse_byte(send[1]);

                    send[2] = (byte)(send[2] << 4);
                    send[1] = (byte)(send[1] << 4);

                    SendToInterface(send);
                    //Thread.Sleep(10);
                }

                public void SendData(byte d)
                {
                    byte temp = 0xFA;
                    byte[] send = new byte[3];
                    send[0] = temp;
                    send[2] = (byte)(d & 0xF0);
                    send[1] = (byte)(d << 4);

                    send[2] = (byte)reverse_byte(send[2]);
                    send[1] = (byte)reverse_byte(send[1]);

                    send[2] = (byte)(send[2] << 4);
                    send[1] = (byte)(send[1] << 4);

                    SendToInterface(send);
                    //Thread.Sleep(10);
                }

                public void init_oled()
                {
                    //_resetPin.Write(false);
                    OutputPort.Write(_resetPin, PinValue.Low);
                    Thread.Sleep(50);
                    OutputPort.Write(_resetPin, PinValue.High);
                    //_resetPin.Write(true);
                    Thread.Sleep(50);

                    SendCommand(0x2A); //function set (extended command set)
                    SendCommand(0x71); //function selection A
                    SendData(0x00); // disable internal VDD regulator (2.8V I/O). data(0x5C) = enable regulator (5V I/O)
                    SendCommand(0x28); //function set (fundamental command set)
                    SendCommand(0x08); //display off, cursor off, blink off
                    SendCommand(0x2A); //function set (extended command set)
                    SendCommand(0x79); //OLED command set enabled
                    SendCommand(0xD5); //set display clock divide ratio/oscillator frequency
                    SendCommand(0x70); //set display clock divide ratio/oscillator frequency
                    SendCommand(0x78); //OLED command set disabled
                    SendCommand(0x09); //extended function set (4-lines)
                    SendCommand(0x06); //COM SEG direction
                    SendCommand(0x72); //function selection B
                                       //SendData(CODEPAGE);// ROM/CGRAM selection: CGROM=250, CGRAM=6 (ROM=10, OPR=10) ROM: A: 0x02, B: 0x06, C: 0x0A
                    SendData(0X05);// ROM/CGRAM selection: CGROM=250, CGRAM=6 (ROM=10, OPR=10) ROM: A: 0x02, B: 0x06, C: 0x0A
                    SendCommand(0x2A); //function set (extended command set)
                    SendCommand(0x79); //OLED command set enabled
                    SendCommand(0xDA); //set SEG pins hardware configuration
                    SendCommand(0x10); //set SEG pins hardware configuration
                    SendCommand(0xDC); //function selection C
                    SendCommand(0x00); //function selection C
                    SendCommand(0x81); //set contrast control
                    SendCommand(0x7F); //set contrast control
                    SendCommand(0xD9); //set phase length
                    SendCommand(0xF1); //set phase length
                    SendCommand(0xDB); //set VCOMH deselect level
                    SendCommand(0x40); //set VCOMH deselect level
                    SendCommand(0x78); //OLED command set disabled
                    SendCommand(0x28); //function set (fundamental command set)
                    SendCommand(0x01); //clear display
                    Thread.Sleep(2);
                    SendCommand(0x80); //set DDRAM address to 0x00
                    SendCommand(0x0C); //display ON
                    Thread.Sleep(100);           // Waits 100 ms for stabilization purpose after display on
                }

                /********** high level commands, for the user! */

                /// <summary>
                /// As the function-name tells you, this clears the screen.
                /// </summary>
                /// <param name="I2C_DEV"></param>
                public void ClearScreen()
                {
                    SendCommand(0x01);
                }



                public void DisplayScrollRight()
                {
                    uint a = 19;
                    t = 100;

                    for (uint i = 0; i < 20; i++)
                    {
                        {
                            for (uint j = 0; j < 4; j++)
                            {
                                DisplayCustom(a, j, 0X20);
                            }
                        }

                        scrollDisplayRight();

                        t = t - 10;

                        if (t <= 10)
                            t = 20;

                        Thread.Sleep(t);

                        a--;
                    }
                }

                public void DisplayScrollLeft()
                {
                    t = 100;

                    for (uint i = 0; i < 20; i++)
                    {
                        for (uint j = 0; j < 4; j++)
                        {
                            DisplayCustom(i, j, 0X20);
                        }

                        scrollDisplayLeft();

                        t = t - 10;

                        if (t <= 10)
                            t = 20;

                        Thread.Sleep(t);
                    }
                }

                public void ScrollArrayLeftStartEnd(byte[] messageArray, uint LineId, uint startCol, uint endCol, int Time, bool fill = false, uint byteFill = 0XBB)
                {
                    {
                        t = 0;

                        for (uint j = startCol; j-- > endCol;)
                        {
                            if (fill)
                            {
                                FillWithSpecial(0, 20, LineId, byteFill);
                            }

                            DisplayArray((int)j, (int)LineId, messageArray);

                            if (!fill)
                            {
                                DisplayCustom(((uint)(j + messageArray.Length)), LineId, 0XBB);//blank char
                            }

                            if (t >= Time)
                            {
                                t = Time - 10;
                            }

                            Thread.Sleep(Time - t);

                            t = t + 10;
                        }
                    }
                }

                public void ScrollArrayRightStartEnd(byte[] messageArray, uint LineId, uint startCol, uint endCol, int Time, bool fill = false, uint byteFill = 0XBB)
                {
                    {
                        t = 0;
                        //for (uint j = startCol; j < 20 - messageArray.Length; j++)
                        for (uint j = startCol; j < endCol; j++)
                        {
                            if (fill)
                            {
                                FillWithSpecial(0, 20, LineId, byteFill);
                            }

                            DisplayArray((int)j, (int)LineId, messageArray);

                            if (!fill)
                            {
                                DisplayCustom((j - 1), LineId, 0XBB);
                            }

                            if (t >= Time)
                            {
                                t = Time - 10;
                            }

                            Thread.Sleep(Time - t);

                            t = t + 10;
                        }
                    }
                }

                public void ScrollArrayLeft(byte[] messageArray, uint LineId, int Time, bool fill = false, uint byteFill = 0XBB)
                {
                    ScrollArrayLeftStartEnd(messageArray, LineId, 21, 0, Time, fill, byteFill);
                }

                public void ScrollArrayRight(byte[] messageArray, uint LineId, int Time, bool fill = false, uint byteFill = 0XBB)
                {
                    ScrollArrayRightStartEnd(messageArray, LineId, 0, (uint)(21 - messageArray.Length), Time, fill, byteFill);
                }

                /// <summary>
                /// Display a single line, the screen is not cleared, so it also can be used to update a single line.
                /// </summary>
                /// <param name="I2C_DEV"></param>
                /// <param name="Line"></param>
                /// <param name="LineID"></param>
                public void DisplayLine(string Line, int LineID)
                {
                    if (LineID >= DISPLAY_LINES || LineID < 0)
                    {
                        LineID = 0;
                    }//Makes sure, the LinID has a valid value
                    while (Line.Length < DISPLAY_CHARS)
                    {
                        Line = Line + " ";
                    }//Makes sure, the rest of the line is blank, if you use this to update a line.
                    char[] cha = Line.ToCharArray();
                    SendCommand(Line_Adresses[LineID]);
                    int CharID = 0;
                    foreach (char c in cha)
                    {
                        if (CharID >= DISPLAY_CHARS)
                            break;
                        SendData((byte)c);
                        CharID++;
                    }
                }

                /// <summary>
                /// Displays a string-array with up to four strings (can be more, but only the first four lines will be displayed).
                /// If there are less than four lines, the content is aligned at the top.
                /// </summary>
                /// <param name="Display"></param>
                public void DisplayAll(string[] Display)
                {
                    ArrayList AList = new ArrayList();
                    foreach (string s in Display)
                    {
                        AList.Add((char[])s.ToCharArray());
                    }
                    SendCommand(0x01);
                    int LineID = 0;
                    foreach (char[] cha in AList)
                    {
                        if (LineID >= DISPLAY_LINES)
                            break;
                        SendCommand(Line_Adresses[LineID]);
                        int CharID = 0;
                        foreach (char c in cha)
                        {
                            if (CharID >= DISPLAY_CHARS)
                                break;
                            SendData((byte)c);
                            CharID++;
                        }
                        LineID++;
                    }
                }

                public void IncColLineX(uint line, uint X)
                {
                    switch (line)
                    {
                        case 0:
                            {
                                currentColLine0 = (X + 1);
                            }
                            break;
                        case 1:
                            {
                                currentColLine1 = (X + 1);
                            }
                            break;
                        case 2:
                            {
                                currentColLine2 = (X + 1);
                            }
                            break;
                        case 3:
                            {
                                currentColLine3 = (X + 1);
                            }
                            break;
                    }
                }

                public void ClearDisplay()
                {
                    SendCommand(LCD_CLEARDISPLAY);  // clear display, set cursor position to zero
                    currentColLine0 = 0;
                    currentColLine1 = 0;
                    currentColLine2 = 0;
                    currentColLine3 = 0;
                    Thread.Sleep(10);
                }

                public void CursorHome()
                {
                    SendCommand(LCD_RETURNHOME);  // set cursor position to zero
                    currentColLine0 = 0;
                    currentColLine1 = 0;
                    currentColLine2 = 0;
                    currentColLine3 = 0;
                }

                public void ClearCol()//requires previous Cursor-set
                {
                    byte[] dd = (Encoding.UTF8.GetBytes(" "));

                    for (int c = 0; c < dd.Length; c++)
                    {
                        SendData(dd[c]);
                    }
                }

                public void ClearCol(uint col, uint line)
                {
                    byte[] dd = (Encoding.UTF8.GetBytes(" "));

                    SetCursor(col, line);

                    for (int c = 0; c < dd.Length; c++)
                    {
                        SendData(dd[c]);
                    }
                }

                public void ClearInterval(uint start, uint end, int LineID)
                {
                    uint interval = (end - start) + 1;
                    string Line = " ";

                    if (LineID >= DISPLAY_LINES || LineID < 0)
                    {
                        LineID = 0;
                    }//Makes sure, the LinID has a valid value

                    while (Line.Length < interval)
                    {
                        Line = Line + " ";
                    }//Makes sure, the rest of the line is blank, if you use this to update a line.

                    OledWriteLine(start, (uint)LineID, Line);
                }

                public void FillWithSpecial(uint start, uint length, uint line, uint schar)
                {
                    SetCursor(start, line);

                    for (uint c = 0; c < length; c++)
                    {
                        SendData((byte)(schar));
                    }
                }

                public byte[] ConvertString(string message)
                {
                    byte[] dd = (Encoding.UTF8.GetBytes(message));

                    return dd;
                }

                /// <summary>
                /// Displays a byte-array with RAM character addresses.
                /// If the array or part of it is out the limits (00-19) the part out is not shown
                /// Returns -1 if fully hidden at a left side, +1 if fully hidden on right side and 0 if normal.
                /// </summary>
                /// <param name="startPosition"></param>
                /// <param name="lineId"></param>
                /// <param name="messageArray"></param>
                public int DisplayArray(int startPosition, int lineId, byte[] messageArray)
                {
                    uint cursor = 0;
                    int a = 0;
                    int b = 0;

                    int min = (0 - messageArray.Length);

                    if (startPosition <= min)
                    {
                        startPosition = min;
                    }
                    else if (startPosition >= 20)
                    {
                        startPosition = 20;
                    }

                    if (startPosition <= 0)
                    {
                        cursor = 0;
                    }
                    else
                    {
                        cursor = (uint)startPosition;
                    }

                    if (cursor >= 19)
                    {
                        cursor = 19;
                    }

                    if (lineId >= 3)
                    {
                        lineId = 3;
                    }

                    SetCursor(cursor, (uint)lineId);

                    if ((startPosition + messageArray.Length) >= 19)
                    {
                        a = (20 - startPosition);
                        if (a >= messageArray.Length)
                        {
                            a = messageArray.Length;
                        }
                        for (int i = 0; i < (int)a; i++)
                        {
                            SendData(messageArray[i]);
                        }
                        if (a == 0)
                            b = 1;
                        //Debug.Print("RIGHT " + a.ToString());
                    }
                    else if (startPosition <= 0)
                    {
                        a = (messageArray.Length - (startPosition + messageArray.Length));

                        if (a >= messageArray.Length)
                        {
                            a = messageArray.Length;
                        }
                        for (int i = a; i < messageArray.Length; i++)
                        {
                            SendData(messageArray[i]);
                        }
                        if (a == messageArray.Length)
                            b = -1;
                        //Debug.Print("LEFT " + a.ToString());
                    }
                    else
                    {
                        for (int i = 0; i < messageArray.Length; i++)
                        {
                            SendData(messageArray[i]);
                        }
                        b = 0;
                        //Debug.Print("normal pointer= " + startPosition.ToString());
                    }

                    return b;
                }

                public void OledWriteLine(uint col, uint line, string displayLine)
                {
                    //Debug.Print(displayLine);
                    try
                    {
                        byte[] dd = (Encoding.UTF8.GetBytes(displayLine));

                        SetCursor(col, line);

                        for (int c = 0; c < displayLine.Length; c++)
                        {
                            SendData(dd[c]);
                        }

                        //currentColLine0 =(uint) (displayLine.Length + col + 1);
                        IncColLineX(line, (uint)(displayLine.Length + col + 1));
                    }
                    catch (Exception)
                    {
                        //Debug.Print("error in OledWriteLine ");
                    }
                }

                public void WriteStringAt(uint col, uint line, string displayLine)
                {
                    byte[] dd = (Encoding.UTF8.GetBytes(displayLine));

                    SetCursor(col, line);

                    for (int c = 0; c < displayLine.Length; c++)
                    {
                        SendData(dd[c]);
                    }
                }

                public void SetCursor(uint col, uint row)
                {
                    byte[] row_offsets = { 0x80, 0xA0, 0xC0, 0xE0 };

                    if (row >= _numlines)
                    {
                        row = 0;  //write to first line if out off bounds
                    }

                    if (col >= _numcols)
                    {
                        col = 0;
                    }

                    SendCommand((byte)(LCD_SETDDRAMADDR | ((col) + row_offsets[row])));
                }

                //Turn the display on/off (quickly)

                /// <summary>
                /// Turn on the display
                /// </summary>
                public void DisplayOn()
                {
                    _displaycontrol = LCD_DISPLAYON;//= 0x04;
                    SendCommand((byte)(LCD_DISPLAYCONTROL | _displaycontrol));
                }

                /// <summary>
                /// Turn off the display
                /// </summary>

                public void DisplayOff()
                {
                    _displaycontrol = LCD_DISPLAYOFF;//= 0x00;
                    SendCommand((byte)(LCD_DISPLAYCONTROL | _displaycontrol));
                }

                // Turns the underline cursor on/off

                /// <summary>
                /// Turn on the underline cursor
                /// </summary>
                public void UnderlineCursorOn()
                {
                    _displaycontrol = LCD_CURSORON;
                    SendCommand((byte)(LCD_DISPLAYCONTROL | _displaycontrol));
                }

                /// <summary>
                /// Turn off the underline cursor
                /// </summary>
                public void UnderlineCursorOff()
                {
                    _displaycontrol = LCD_CURSOROFF;
                    SendCommand((byte)(LCD_DISPLAYCONTROL | _displaycontrol));
                }

                // Turn on and off the blinking cursor

                /// <summary>
                /// Turn the blinking cursor on
                /// </summary>
                public void BlinkingCursorOn()
                {
                    _displaycontrol = LCD_BLINKON;
                    SendCommand((byte)(LCD_DISPLAYCONTROL | _displaycontrol));
                }

                /// <summary>
                /// Turn the blinking cursor off
                /// </summary>
                public void BlinkingCursorOff()
                {
                    _displaycontrol = LCD_BLINKOFF;
                    SendCommand((byte)(LCD_DISPLAYCONTROL | _displaycontrol));
                }

                /// <summary>
                /// Move the cursor left
                /// </summary>
                public void MoveCursorLeft()
                {
                    //if (currentCol.isBegin())
                    {
                        //SendToInterface(new byte[] { 0xF8, 0x80, 0X09 });
                        SendCommand(0x80);
                        SendData(0x09);
                        //currentCol--;
                    }
                }

                /// <summary>
                /// Move the cursor right
                /// </summary>
                public void MoveCursorRight()
                {
                    //if (currentCol.isEnd())
                    {
                        //SendToInterface(new byte[] { 0xF8, 0x4A });
                        SendCommand(0x4A);
                        //currentCol++;
                    }
                }

                // These commands scroll the display without changing the RAM
                public void scrollDisplayLeft()//ok
                {
                    SendCommand(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVELEFT);
                }

                public void scrollDisplayRight()//ok
                {
                    SendCommand(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVERIGHT);
                }

                // This is for text that flows Left to Right
                public void leftToRight()
                {
                    _displaymode |= LCD_ENTRYLEFT;
                    SendCommand((byte)(LCD_ENTRYMODESET | _displaymode));
                }



                // This will 'right justify' text from the cursor
                public void autoscroll()
                {
                    _displaymode |= LCD_ENTRYSHIFTINCREMENT;
                    SendCommand((byte)(LCD_ENTRYMODESET | _displaymode));
                }



                public void setBrightness(uint value)
                {
                    SendCommand(0x80);        // set RE=1
                    SendCommand(0x2A);

                    SendCommand(0x80);        // set SD=1
                    SendCommand(0x79);

                    SendCommand(OLED_SETBRIGHTNESSCOMMAND);
                    SendCommand((byte)value);

                    SendCommand(0x80);        // set SD=0
                    SendCommand(0x78);

                    SendCommand(0x80);        // set RE=0
                    SendCommand(0x28);
                }

                // Allows us to fill the first 8 CGRAM locations
                // with custom characters
                public void createChar(uint location, byte[] charmap)
                {
                    location &= 0x7; // we only have 8 locations 0-7
                    SendCommand((byte)(LCD_SETCGRAMADDR | (location << 3)));
                    for (int i = 0; i < 8; i++)
                    {
                        SendData((byte)charmap[i]);
                    }
                }

                public void DisplayCustom(uint col, uint LineID, uint CharID)
                {
                    if (LineID >= DISPLAY_LINES || LineID < 0)
                    {
                        LineID = 0;
                    }//Makes sure, the LinID has a valid value

                    SetCursor(col, LineID);

                    SendData((byte)CharID);
                }
            }
        }
    }


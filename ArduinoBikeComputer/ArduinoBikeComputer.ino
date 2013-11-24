#include <i2cmaster.h>

int sense0 = 2;
int sense1 = 3;
int counter0 = 0;
int counter1 = 0;
unsigned long currentTime = 0;
unsigned long lastDebounce0 = 0;
unsigned long lastDebounce1 = 0;
double lastRPM1 = 0.0;
long debounceDelay = 60;    // Ignore bounces 60 millisecond ... 150 rpm on pedals ~ 128 ms per wheel rotation
long tempDelay = 1000;

void setup()
{
  Serial.begin(9600);
  //Serial.println("Setup...");
  	
  TempSetup();
  RPMSetup();

  currentTime = millis();
 
 
} 
       
void loop()
{
  long curr = millis();
  if ( currentTime + tempDelay < curr)
  {
    Serial.print("### Temp ### W:");
    Serial.print( TemperatureSensing() );
    Serial.print("~t:");
    Serial.println(curr);
    delay(1000); // wait a second before printing again
  }
}

void RPMSetup()
{
  pinMode(sense0, INPUT);
  digitalWrite(sense0, HIGH);
  pinMode(sense1, INPUT);
  digitalWrite(sense1, HIGH);
  attachInterrupt(0, trigger0, FALLING);
  attachInterrupt(1, trigger1, FALLING);
  //Serial.println("Repetition counter");
  //Serial.print("Start");
  //Serial.print("\t");
  //Serial.println("End");
}

void TempSetup()
{
  i2c_init(); //Initialise the i2c bus
  PORTC = (1 << PORTC4) | (1 << PORTC5);//enable pullups
}

void trigger0()
{
  currentTime = millis();
  if( (currentTime - lastDebounce0) > debounceDelay){
    counter0++;
    //Serial.print(counter0);
    //Serial.print(" : ");
    //Serial.print(counter1);
    
    //Serial.print(" ::: ");
    //Serial.print( CalcPedalRPM( currentTime, lastDebounce0 ) );
    //Serial.print(" : ");
    //Serial.println( lastRPM1 );
    float tempC = TemperatureSensing();
    float pedalRpm = CalcPedalRPM( currentTime, lastDebounce0 );

    Serial.print( "C:" );
    Serial.print( pedalRpm );
    Serial.print( "~" );
    
    Serial.print( "W:" );
    Serial.print( tempC );
    Serial.print( "~" );
    
    Serial.print( "t:" );
    Serial.print( currentTime );
    
    
    Serial.println();
    
    lastDebounce0 = currentTime;

  }
}

double CalcPedalRPM(unsigned long curr, unsigned long last)
{
  //  ( 1 wheelrot / (curr-last)usec ) * ( 10^3 usec / 1 s) * ( 60 sec / 1 min) * (22.0 pedalrot / 68.8 wheelrot);
  return 19186.046511628 / ((double)(curr - last)) ;
}

void trigger1()
{
  currentTime = millis();
  if( (currentTime - lastDebounce1) > debounceDelay)
  {
    counter1++;
    lastRPM1 = CalcPedalRPM( currentTime, lastDebounce1 );  
    lastDebounce1 = currentTime;
  }
}

double TemperatureSensing()
{
    int dev = 0x5A<<1;
    int data_low = 0;
    int data_high = 0;
    int pec = 0;
    
    i2c_start_wait(dev+I2C_WRITE);
    i2c_write(0x07);
    
    // read
    i2c_rep_start(dev+I2C_READ);
    data_low = i2c_readAck(); //Read 1 byte and then send ack
    data_high = i2c_readAck(); //Read 1 byte and then send ack
    pec = i2c_readNak();
    i2c_stop();
    
    //This converts high and low bytes together and processes temperature, MSB is a error bit and is ignored for temps
    double tempFactor = 0.02; // 0.02 degrees per LSB (measurement resolution of the MLX90614)
    double tempData = 0x0000; // zero out the data
    int frac; // data past the decimal point
    
    // This masks off the error bit of the high byte, then moves it left 8 bits and adds the low byte.
    tempData = (double)(((data_high & 0x007F) << 8) + data_low);
    tempData = (tempData * tempFactor)-0.01;
    
    double celcius = tempData - 273.15;
    //double fahrenheit = (celcius*1.8) + 32;

    //Serial.print("Celcius: ");
    //Serial.println(celcius);

    //Serial.print("Fahrenheit: ");
    //Serial.println(fahrenheit); 
    
    return celcius;
}

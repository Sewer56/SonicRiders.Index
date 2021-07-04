```c
//------------------------------------------------
//--- 010 Editor v11.0 Binary Template
//
//      File: Sonic Riders Object Portal File (ID 00301 / Obj 00000)
//   Authors: Sewer56
//   Version: 1.0.0
// Description: File format for "portals". 
//              Bounding boxes which define when objects should be culled in and out.
//------------------------------------------------

/*
    Note: ID 306 also contains this format but is unused.
*/

struct Vector3Int
{
    int x;
    int y;
    int z;
};


struct Portal
{
    short startX;
    short startY;
    short startZ;

    short endX;
    short endY;
    short endZ;

    char portalChar;
    byte isRotated;

    byte padding[2];

    // Range 0 - 65535
    // i.e. 13684 = 90 degrees
    Vector3Int rotationBams; 
};

// File Data Starts Here
byte numOfPortals;
byte unknown;

byte firstPortalChar;
byte lastPortalChar;

// Padding
FSeek(FTell() + 4);

// Portals.
Portal portals[numOfPortals];
```
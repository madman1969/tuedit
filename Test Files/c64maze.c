#include "c64maze.h"

char labyrinth[] = "****************************************"
                   "*      *    *     * *   *        *     *"
                   "* **** * ******** * *** * *** **   ** **"
                   "* * ** *   *  *   *     *   * *  *  *  *"
                   "*   *  * *  * *** ***** * * * * ***  * *"
                   "***** ** ** *             *** *   **** *"
                   "*             * ** **** * *     *  *   *"
                   "* * *********** *     * * * ****** * * *"
                   "* *         *   ** ** *   * *        * *"
                   "***** ***** * * *  *  * * * **** ** *  *"
                   "* *       * *** **** ** **     *  * ****"
                   "* * *** * * *     *  *    * ** * *     *"
                   "*     *** * ***** * ** **** *  * **** **"
                   "** ** *   *     * * *   *   * **    *  *"
                   "*  * ** ******* * * *** * *** *  ** ** *"
                   "**   *  *         * *   *      * *     *"
                   "****************************************";

static unsigned char pix_pos[] = {0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01};

char startx;
char starty;
char positionx;
char positiony;
unsigned char style = 0x1;

char exitx = 13;
char exity = 1;
/*  0 = north
        1 = west
        2 = sud
        3 = east
    */
char orientation = 0;

// Is this ever used ? ...
struct font f;

/* Turn on a pixel by accessing directly to the video RAM */
void pset(unsigned int x, unsigned int y)
{
    static unsigned int d;
    static unsigned int e;
    static unsigned int by;
    d = y & 0xFFF8;
    e = d * 40;
    by = BASE + e + (x & 0xFFF8) + ((unsigned char)y & 7);
    POKE(by, PEEK(by) | pix_pos[(unsigned char)x & 7]);
}

void clearHGRpage(void)
{
    //unsigned int i;
    // Clear HGR page (too slow!)
    /*for(i=BASE;i<BASE+8000;++i)
        POKE(i,0);*/
    /* The fastest version you can possibly imagine, with an unrolled loop .
       Do not forget to change the addresses if you need to modify the base
       address of the HGR page */
    asm("       ldy #0");
    asm("       tya");
    asm("loop:  sta $A000,y");
    asm("       sta $A100,y");
    asm("       sta $A200,y");
    asm("       sta $A300,y");
    asm("       sta $A400,y");
    asm("       sta $A500,y");
    asm("       sta $A600,y");
    asm("       sta $A700,y");
    asm("       sta $A800,y");
    asm("       sta $A900,y");
    asm("       sta $AA00,y");
    asm("       sta $AB00,y");
    asm("       sta $AC00,y");
    asm("       sta $AD00,y");
    asm("       sta $AE00,y");
    asm("       sta $AF00,y");
    asm("       sta $B000,y");
    asm("       sta $B100,y");
    asm("       sta $B200,y");
    asm("       sta $B300,y");
    asm("       sta $B400,y");
    asm("       sta $B500,y");
    asm("       sta $B600,y");
    asm("       sta $B700,y");
    asm("       sta $B800,y");
    asm("       sta $B900,y");
    asm("       sta $BA00,y");
    asm("       sta $BB00,y");
    asm("       sta $BC00,y");
    asm("       sta $BD00,y");
    asm("       sta $BE00,y");
    asm("       sta $BE40,y");
    asm("       iny");
    asm("       bne loop");

    // Setup color memory
    /*for(i=COLOR_MEM;i<COLOR_MEM+1000;++i)
        POKE(i,3);*/
    asm("       ldy #0");
    asm("       lda #3");
    asm("loop1: sta $8C00,y");
    asm("       sta $8D00,y");
    asm("       sta $8E00,y");
    asm("       sta $8F00,y");
    asm("       iny");
    asm("       bne loop1");
}

/* printat prints a text at the given location unsing the font described
   by the structure font */
void printat(unsigned short x, unsigned short y, char *s)
{
    unsigned char i, j, k;
    unsigned char a;
    unsigned char t;
    unsigned int p;
    unsigned char mm = f.magnification;
    unsigned char incrementx = f.incX * mm;
    unsigned char incrementy = f.incY * mm;
    unsigned int ppos;
    unsigned char q, r;

    unsigned int by;
    unsigned int d, e;
    unsigned int ix;
    unsigned int loc;
    for (i = 0; s[i] != '\0'; ++i)
    {
        p = 0;
        r = 1;
        ppos = f.pos[s[i]];
        x += incrementx;
        for (j = 0; j < incrementy; ++j)
        {
            a = f.pDesc[ppos + p];
            if (r == mm)
            {
                ++p;
                r = 1;
            }
            else
            {
                ++r;
            }
            t = 1;
            q = y + j;
            d = q & 0xFFF8;
            e = d * 40;
            by = BASE + e + ((unsigned char)q & 7);
            for (k = 0; a != 0; ++k)
            {
                if (a & 0x0001)
                {
                    ix = x - k;
                    loc = by + (ix & 0xFFF8);
                    POKE(loc, PEEK(loc) | pix_pos[(unsigned char)ix & 7]);
                }
                if (t == mm)
                {
                    a >>= 1;
                    t = 1;
                }
                else
                {
                    ++t;
                }
            }
        }
    }
}

void clearMazeRegion(void)
{
    /*unsigned char x;
    unsigned char y;
    unsigned int by=BASE;
    // Clear the leftmost part of the screen

    for(y=0;y<25;++y) {
        for(x=0;x<200;++x)
            POKE(by++,0);
        by+=120;
    }*/
    // Very fast version of the routine.
    asm("       ldy #0");
    asm("       tya");
    asm("loop2:  sta $A000,y");
    asm("       sta $A000+320,y");
    asm("       sta $A000+2*320,y");
    asm("       sta $A000+3*320,y");
    asm("       sta $A000+4*320,y");
    asm("       sta $A000+5*320,y");
    asm("       sta $A000+6*320,y");
    asm("       sta $A000+7*320,y");
    asm("       sta $A000+8*320,y");
    asm("       sta $A000+9*320,y");
    asm("       sta $A000+10*320,y");
    asm("       sta $A000+11*320,y");
    asm("       sta $A000+12*320,y");
    asm("       sta $A000+13*320,y");
    asm("       sta $A000+14*320,y");
    asm("       sta $A000+15*320,y");
    asm("       sta $A000+16*320,y");
    asm("       sta $A000+17*320,y");
    asm("       sta $A000+18*320,y");
    asm("       sta $A000+19*320,y");
    asm("       sta $A000+20*320,y");
    asm("       sta $A000+21*320,y");
    asm("       sta $A000+22*320,y");
    asm("       sta $A000+23*320,y");
    asm("       sta $A000+24*320,y");
    asm("       sta $A000+25*320,y");
    asm("       iny");
    asm("       cpy #200");
    asm("       bne loop2");
}

/** Switch on the HGR monochrome graphic mode.
*/
void graphics_monochrome(void)
{
    POKE(56576U, 0x01);
    POKE(53272U, 0x38);
    POKE(53265U, 0x36);
    clearHGRpage();
}

void vert_line(unsigned short x1, unsigned short y1, unsigned short y2)
{
    static unsigned int d;
    static unsigned int e;
    static unsigned int by;
    static char v;
    static unsigned int cc;

    cc = BASE + (x1 & 0xFFF8);
    v = pix_pos[(unsigned char)x1 & 7];
    for (; y1 <= y2; ++y1)
    {
        if ((y1 & 7) == 0 && (y1 + 8) <= y2)
        {
            d = y1 & 0xFFF8;
            e = d * 40;
            by = cc + e;
            POKE(by, PEEK(by++) | v);
            POKE(by, PEEK(by++) | v);
            POKE(by, PEEK(by++) | v);
            POKE(by, PEEK(by++) | v);
            POKE(by, PEEK(by++) | v);
            POKE(by, PEEK(by++) | v);
            POKE(by, PEEK(by++) | v);
            POKE(by, PEEK(by++) | v);
            y1 += 7; // because the for statement will add 1.
            continue;
        }
        d = y1 & 0xFFF8;
        e = d * 40;
        by = cc + e + ((unsigned char)y1 & 7);
        POKE(by, PEEK(by) | v);
    }
}

void diag_line(unsigned short x1, unsigned short y1, unsigned short ix,
               short incx, short incy)
{
    static unsigned int d;
    static unsigned int e;
    static unsigned int by;
    static int i;
    for (i = 0; i <= ix; ++i)
    {
        d = y1 & 0xFFF8;
        e = d * 40;
        by = BASE + e + (x1 & 0xFFF8) + ((unsigned char)y1 & 7);
        POKE(by, PEEK(by) | pix_pos[(unsigned char)x1 & 7]);
        if (incx > 0)
            ++x1;
        else
            --x1;
        if (incy > 0)
            ++y1;
        else
            --y1;
    }
}

void hor_line(unsigned short x1, unsigned short x2, unsigned short y1)
{
    static unsigned int d;
    static unsigned int e;
    static unsigned int by;
    d = y1 & 0xFFF8;
    e = d * 40;
    by = BASE + e + ((unsigned char)y1 & 7);
    for (; x1 <= x2; ++x1)
    {
        if ((x1 & 7) == 0 && (x1 + 7) <= x2)
        {
            POKE(by + (x1 & 0xFFF8), 0xFF);
            x1 += 7; // because the for statement will add 1.
            continue;
        }
        POKE(by + (x1 & 0xFFF8), PEEK(by + (x1 & 0xFFF8)) |
                                     pix_pos[(unsigned char)x1 & 7]);
    }
}

/* Plot a line using the Bresenham algorithm
   from Nelson Johnson, "Advanced Graphics in C"
   ed. Osborne, McGraw-Hill.
   Horizontal and vertical lines need to be considerably
   speeded up by using a direct access to video RAM.
*/
void line(unsigned short x1, unsigned short y1,
          unsigned short x2, unsigned short y2)
{
    static short incx;
    static short incy;

    static unsigned short ix;
    static unsigned short iy;
    static unsigned short inc;

    static unsigned char changey;
    static short x, y;
    static unsigned char plot;

    static unsigned int d;
    static unsigned int e;
    static unsigned int by;
    static unsigned int ypos;
    static unsigned int i;
    static unsigned char style_mask;

    incx = x2 > x1 ? 1 : -1;
    incy = y2 > y1 ? 1 : -1;

    ix = incx > 0 ? x2 - x1 : x1 - x2;
    iy = incy > 0 ? y2 - y1 : y1 - y2;

    // If continuous lines have to be drawn, check if we can employ simplified
    // and faster code in certain particular cases.
    if (style == 0x1)
    {
        if (ix == 0)
        {
            if (incy > 0)
                vert_line(x1, y1, y2);
            else
                vert_line(x1, y2, y1);
            return;
        }
        if (iy == 0)
        {
            if (incx > 0)
                hor_line(x1, x2, y1);
            else
                hor_line(x2, x1, y1);
            return;
        }
        if (ix == iy)
        {
            diag_line(x1, y1, ix, incx, incy);
            return;
        }
    }

    inc = a_max(ix, iy);
    style_mask = style;

    changey = TRUE;
    x = 0;
    y = 0;
    plot = FALSE;

    d = y1 & 0xFFF8;
    e = d * 40;
    by = BASE + e + (x1 & 0xFFF8) + ((unsigned char)y1 & 7);

    /* Plot the first pixel */
    POKE(by, PEEK(by) | pix_pos[(unsigned char)x1 & 7]);

    /* Faster version with continuous line */
    for (i = 0; i <= inc; ++i)
    {
        x += ix;
        y += iy;
        if (x > inc)
        {
            plot = TRUE;
            x -= inc;
            x1 += incx;
        }
        if (y > inc)
        {
            plot = TRUE;
            y -= inc;
            y1 += incy;
            changey = TRUE;
        }
        if (plot && (style_mask & 0x0001u))
        {
            plot = FALSE;
            if (changey == TRUE)
            {
                /* Calculations for the position of the memory location to
                   modify are more complicated in y than in x, so it is
                   worth doing them only when necessary. That greatly
                   improves speed for almost horisontal lines. */
                changey = FALSE;
                d = y1 & 0xFFF8;
                e = d * 40;
                ypos = BASE + e + ((unsigned char)y1 & 0x07);
            }
            by = ypos + (x1 & 0xFFF8);
            POKE(by, PEEK(by) | pix_pos[(unsigned char)x1 & 0x07]);
        }
        style_mask >>= 1;
        if (style_mask == 0)
            style_mask = style;
    }
}

/* box draws a box given the coordinates of the two
   diagonal corners. From Nelson Johnson, "Advanced
   Graphics in C" ed. Osborne, McGraw-Hill
 */
void box(unsigned short x1, unsigned short y1, unsigned short x2,
         unsigned short y2)
{
    static unsigned short xul;
    static unsigned short yul;
    static unsigned short xlr;
    static unsigned short ylr;

    xul = (x2 > x1) ? x1 : x2;
    yul = (y2 > y1) ? y1 : y2;
    xlr = (x2 > x1) ? x2 : x1;
    ylr = (y2 > y1) ? y2 : y1;
    // The simplified code does not handle every style lines, so one must check
    // for it.
    if (style == 0x1)
    {
        hor_line(xul, xlr, yul);
        hor_line(xul, xlr, ylr);
        vert_line(xul, yul, ylr);
        vert_line(xlr, yul, ylr);
    }
    else
    {
        line(xul, yul, xlr, yul);
        line(xlr, yul, xlr, ylr);
        line(xlr, ylr, xul, ylr);
        line(xul, ylr, xul, yul);
    }
}

void loadVICFont(unsigned char magnification)
{
    unsigned int i;
    /* Load the font tables */
    for (i = 0; i < 256; ++i)
        f.pos[i] = 0;

    for (i = ' '; i <= '~'; ++i)
        f.pos[i] = (i - ' ' + 1) * 8;

    f.pDesc = vic_font;
    f.incX = 8; /* Increment in X (-1 would mean a proportional font) */
    f.incY = 8; /* Increment in Y */
    f.magnification = magnification;
}

/** Choose randomly the starting position in the maze.
*/
void choose_start_position()
{
    unsigned int time = PEEK(160) + PEEK(161) * 256;
    srand(time);
    do
    {
        startx = (labyrinthSizeX * (rand() / (RAND_MAX / 100))) / 100;
        starty = (labyrinthSizeY * (rand() / (RAND_MAX / 100))) / 100;
    } while (labyrinth[startx + starty * labyrinthSizeX] != ' ');
    positionx = startx;
    positiony = starty;
}

int leftx;
int lefty;
int rightx;
int righty;
int advancex;
int advancey;

void set_orientation(void)
{
    switch (orientation)
    {
    case 0:
        leftx = -1;
        lefty = 0;
        rightx = 1;
        righty = 0;
        advancex = 0;
        advancey = -1;
        break;
    case 1:
        leftx = 0;
        lefty = 1;
        rightx = 0;
        righty = -1;
        advancex = -1;
        advancey = 0;
        break;
    case 2:
        leftx = 1;
        lefty = 0;
        rightx = -1;
        righty = 0;
        advancex = 0;
        advancey = 1;
        break;
    case 3:
        leftx = 0;
        lefty = -1;
        rightx = 0;
        righty = 1;
        advancex = 1;
        advancey = 0;
        break;
    }
}

/** In this function, the maze view is shown from the current point and
    orientation of the player (those are specified in global variables).
*/
void drawLabyrinthView()
{
    unsigned char posx = positionx;
    unsigned char posy = positiony;
    unsigned char step = 0;
    unsigned int sszx;
    unsigned int sszy;
    unsigned int sszxp1;
    unsigned int sszyp1;
    unsigned char wall = FALSE;
    unsigned char wayout = FALSE;

    style = 0x1;
    set_orientation();

    /* Draw the maze in isometric perspective starting from the position of
       the player and going progressively farther from him, until a wall is
       hit or until the distance becomes greater than 5 steps.
    */
    for (step = 0; (wall == FALSE) && (step < 6); ++step)
    {
        /* Specify the line style so that walls and corners far from the
           player somewhat appears to be less definite, thus adding a certain
           3D effect.
        */
        switch (step)
        {
        case 3:
            style = 0x2;
            break;
        case 4:
            style = 0x8;
            break;
        case 5:
            style = 0x20;
            break;
        default:
            style = 0x1;
            break;
        }
        // We hit a wall!
        if (labyrinth[posx + posy * labyrinthSizeX] == '*')
        {
            break;
        }
        // Some pre-calculated data for wall and corner drawing.
        sszx = step * STEPSIZEX;
        sszy = step * STEPSIZEY;
        sszxp1 = (step + 1) * STEPSIZEX;
        sszyp1 = (step + 1) * STEPSIZEY;
        // Wall on the right?
        if (labyrinth[posx + rightx + (posy + righty) * labyrinthSizeX] == '*')
        {
            line(SIZEX - sszx, sszy,
                 SIZEX - sszxp1, sszyp1);
            line(SIZEX - sszx, SIZEY - sszy,
                 SIZEX - sszxp1, SIZEY - sszyp1);
        }
        else
        {
            // Closer vertical line
            line(SIZEX - sszx, sszy,
                 SIZEX - sszx, SIZEY - sszy);
            // Farther vertical line
            if (labyrinth[posx + advancex + (posy + advancey) * labyrinthSizeX] != '*')
            {
                line(SIZEX - sszxp1, sszyp1,
                     SIZEX - sszxp1, SIZEY - sszyp1);
            }
            // Upper horisontal line
            line(SIZEX - sszxp1, sszyp1,
                 SIZEX - sszx, sszyp1);
            // Lower horisontal line
            line(SIZEX - sszxp1, SIZEY - sszyp1,
                 SIZEX - sszx, SIZEY - sszyp1);
        }
        // Wall on the left?
        if (labyrinth[posx + leftx + (posy + lefty) * labyrinthSizeX] == '*')
        {
            line(sszx, sszy,
                 sszxp1, sszyp1);
            line(sszx, SIZEY - sszy,
                 sszxp1, SIZEY - sszyp1);
        }
        else
        {
            // Closer vertical line
            line(sszx, sszy,
                 sszx, SIZEY - sszy);
            // Farter vertical line
            if (labyrinth[posx + advancex + (posy + advancey) * labyrinthSizeX] != '*')
            {
                line(sszxp1, sszyp1,
                     sszxp1, SIZEY - sszyp1);
            }
            // Upper horisontal line
            line(sszxp1, sszyp1,
                 sszx, sszyp1);
            // Lower horisontal line
            line(sszxp1, SIZEY - sszyp1,
                 sszx, SIZEY - sszyp1);
        }
        // Advance one step farther from the player.
        posx += advancex;
        posy += advancey;
        // Exit is in sight!
        if (posx == exitx && posy == exity)
        {
            wall = TRUE;
            wayout = TRUE;
        }
        // A wall is in sight!
        if (labyrinth[posx + posy * labyrinthSizeX] == '*')
        {
            wall = TRUE;
        }
    }
    // We have a wall at the end of our sight
    if (wall == TRUE)
        box(step * STEPSIZEX, step * STEPSIZEY,
            SIZEX - step * STEPSIZEX, SIZEY - step * STEPSIZEY);
    // The exit is in sight!
    if (wayout)
    {
        ++step;
        box(step * STEPSIZEX, step * STEPSIZEY,
            SIZEX - step * STEPSIZEX, SIZEY - step * STEPSIZEY);
        line(step * STEPSIZEX, step * STEPSIZEY,
             SIZEX - step * STEPSIZEX, SIZEY - step * STEPSIZEY);
        line(SIZEX - step * STEPSIZEX, step * STEPSIZEY,
             step * STEPSIZEX, SIZEY - step * STEPSIZEY);
    }
}

/** Be sure that the current player position is correct.
*/
void validate_data()
{
    if (positionx >= labyrinthSizeX)
        positionx = labyrinthSizeX - 1;
    if (positiony >= labyrinthSizeY)
        positiony = labyrinthSizeY - 1;
    if (positiony <= 0)
        positiony = 0;
    if (positiony <= 0)
        positiony = 0;
}

/** Advance in the forward direction, following the current orientation.
*/
void move_forward()
{
    switch (orientation)
    {
    case 0:
        --positiony;
        break;
    case 1:
        --positionx;
        break;
    case 2:
        ++positiony;
        break;
    case 3:
        ++positionx;
        break;
    }
    validate_data();
}

/** Go backwards one step, following the current orientation.
*/
void move_backwards()
{
    switch (orientation)
    {
    case 0:
        ++positiony;
        break;
    case 1:
        ++positionx;
        break;
    case 2:
        --positiony;
        break;
    case 3:
        --positionx;
        break;
    }
    validate_data();
}

/** Colour the right side banner in the screen view.
*/
void colour_banner(void)
{
    unsigned char x;
    unsigned char y;

    for (x = 25; x < 40; ++x)
    {
        for (y = 0; y < 25; ++y)
        {
            POKE(COLOR_MEM + x + y * 40, 0x67);
        }
    }
}

/** Draw the banner on the right side of the screen.
*/
void draw_banner()
{
    colour_banner();
    loadVICFont(2);
    printat(204, 17, "c64maze");
    loadVICFont(1);
    box(251, 53, 278, 84);
    printat(260, 55, "t");
    printat(252, 65, "f+g");
    printat(260, 75, "v");
    printat(207, 100, "[p] view maze");
    printat(207, 110, "[m] music 1/0");
    printat(207, 120, "[a] restart  ");
    printat(207, 170, "d. bucci 2017");
    loadVICFont(2);
    line(200, 0, 200, 199);
}

long start_time;

long get_current_time(void)
{
    long ctime = ((char)PEEK(160));
    ctime <<= 8;
    ctime += ((char)PEEK(161));
    ctime <<= 8;
    ctime += ((char)PEEK(162));
    return ctime;
}

char *write_time(char *message, unsigned char start)
{
    long approx = 0;
    char thousands = 0;
    char hundreds = 0;
    char tens = 0;
    char seconds = 0;
    approx = get_current_time() - start_time;
    if (approx > 60 * 1000)
    {
        thousands = approx / 60l / 1000l;
        approx -= thousands * 60l * 1000l;
    }
    if (approx > 60 * 100)
    {
        hundreds = approx / 60l / 100l;
        approx -= hundreds * 60l * 100l;
    }
    if (approx > 60 * 10)
    {
        tens = approx / 60l / 10l;
        approx -= tens * 60l * 10l;
    }
    if (approx > 60)
    {
        seconds = approx / 60l;
        approx -= seconds * 60l;
    }

    message[start++] = thousands + '0';
    message[start++] = hundreds + '0';
    message[start++] = tens + '0';
    message[start++] = seconds + '0';
    return message;
}

/** Show the maze with the current position and orientation.
*/
void show_maze()
{
    unsigned char x;
    unsigned char y;
    unsigned int by;
    char *pt;

    char message[] = "elapsed:      s";

    style = 0x1;

    start_time -= 60l * 30l; // 30 seconds penalty.

    clearHGRpage();
    for (y = 0; y < labyrinthSizeY; ++y)
    {
        by = COLOR_MEM + y * 40;
        pt = labyrinth + y * labyrinthSizeX;
        for (x = 0; x < labyrinthSizeX; ++x)
        {
            if (pt[x] == '*')
            {
                POKE(by, 9);
            }
            else if (positiony == y && positionx == x)
            {
                box(x * 8 + 2, y * 8 + 2, x * 8 + 5, y * 8 + 5);
                switch (orientation)
                {
                case 0:
                    line(x * 8 + 3, y * 8, x * 8 + 3, y * 8 + 2);
                    line(x * 8 + 4, y * 8, x * 8 + 4, y * 8 + 2);
                    break;
                case 1:
                    line(x * 8, y * 8 + 3, x * 8 + 2, y * 8 + 3);
                    line(x * 8, y * 8 + 4, x * 8 + 2, y * 8 + 4);
                    break;
                case 2:
                    line(x * 8 + 3, y * 8 + 5, x * 8 + 3, y * 8 + 7);
                    line(x * 8 + 4, y * 8 + 5, x * 8 + 4, y * 8 + 7);
                    break;
                case 3:
                    line(x * 8 + 5, y * 8 + 3, x * 8 + 7, y * 8 + 3);
                    line(x * 8 + 5, y * 8 + 4, x * 8 + 7, y * 8 + 4);
                    break;
                }
            }
            ++by;
        }
    }
    f.magnification = 1;
    printat(15, 150, "https://github.com/darwinne/c64maze");
    f.magnification = 2;
    write_time(message, 9);
    printat(40, 170, message);
    cgetc();
    clearHGRpage();
    draw_banner();
}

#define STACK_SIZE 256
unsigned char stackSize[STACK_SIZE];

unsigned int cnt;
unsigned char *list1;
unsigned char *ptr1;
unsigned char wsh1;

unsigned char *list2;
unsigned char *ptr2;
unsigned char wsh2;

unsigned char *list3;
unsigned char *ptr3;
unsigned char wsh3;

void start_game(void)
{
    start_time = get_current_time();
    choose_start_position();
    draw_banner();
}

/** Starting point of the program.
*/
void main(void)
{
    unsigned char oldx = 0;
    unsigned char oldy = 0;
    char oldo = 0;
    char c;
    char iv = TRUE;
    unsigned char music = TRUE;
    char time_spent[] = "     s";

    graphics_monochrome();
restart:
    start_game();

    while (TRUE)
    {
        if (oldx != positionx || oldy != positiony || oldo != orientation)
        {
            oldx = positionx;
            oldy = positiony;
            oldo = orientation;
            clearMazeRegion();
            drawLabyrinthView();
            if (positionx == startx && positiony == starty)
                printat(40, 100, "step in!");
            if (positionx == exitx && positiony == exity)
            {
                printat(40, 70, "way out!");
                write_time(time_spent, 0);
                printat(50, 100, time_spent);
                loadVICFont(1);
                printat(55, 140, "press a key");
                printat(47, 150, "to play again");
                cgetc();
                oldx = 0;
                goto restart; // No program for the C64 would be complete
                              // without at least a GOTO statement somewhere.
            }
            POKE(SCREEN_BORDER, 4);
        }
        do
        {
            c = cgetc();
            iv = FALSE;
            switch (c)
            {
            case 't': // Forward
                move_forward();
                break;
            case 'v': // Backwards
                move_backwards();
                break;
            case 'f': // Turn left
                if (orientation == 3)
                    orientation = 0;
                else
                    ++orientation;
                break;
            case 'g': // Turn right
                if (orientation == 0)
                    orientation = 3;
                else
                    --orientation;
                break;
            case 'p': // Show maze and position
                show_maze();
                // force a redraw.
                oldx = 0;
                break;
            case 'a': // Start again
                clearHGRpage();
                start_game();
                oldx = 0;
                break;
            case 'm': // Toggle music on/off
                if (music == TRUE)
                {
                    POKE(0xD418, 0);
                    music = FALSE;
                }
                else
                {
                    POKE(0xD418, 15);
                    music = TRUE;
                }
                break;
            default:
                iv = TRUE;
            };
        } while (iv == TRUE);
        if (labyrinth[positionx + positiony * labyrinthSizeX] == '*')
        {
            POKE(SCREEN_BORDER, 1);
            positionx = oldx;
            positiony = oldy;
        }
    }
}
﻿namespace Acknex
{
    public partial class World
    {
        private void CreateDefaultSkills()
        {
            CreateSkill("SCREEN_WIDTH", 320, 0, 320);
            CreateSkill("SCREEN_HGT", 400, 0, 400);
            CreateSkill("SCREEN_X", 0, 0, 0); //todo
            CreateSkill("SCREEN_Y", 0, 0, 0); //todo
            CreateSkill("ASPECT", 0, 0, 0); //todo
            CreateSkill("EYE_DIST", 0, 0, 0); //todo
            CreateSkill("SKY_OFFS_X", 0, 0, 0); //todo
            CreateSkill("SKY_OFFS_Y", 0, 0, 0); //todo
            CreateSkill("MOTION_BLUR", 0, 0, 1); //todo
            CreateSkill("BLUR_MODE", 0, 0, 1); //todo
            CreateSkill("RENDER_MODE", 0.5f, 0, 2); //todo
            CreateSkill("MOVE_MODE", 1, -0.5f, 0.5f); //todo
            CreateSkill("CLIPPING", 0, 0, 1); //todo
            CreateSkill("LOAD_MODE", 0, 0, 1); //todo
            CreateSkill("THING_DIST", 1, 0, 1); //todo
            CreateSkill("ACTOR_DIST", 1, 0, 1); //todo
            CreateSkill("MAP_OFFSX", 0, 0, 0); //todo
            CreateSkill("MAP_OFFSY", 0, 0, 0); //todo
            CreateSkill("MAP_CENTERX", 0, 0, 0); //todo
            CreateSkill("MAP_CENTERY", 0, 0, 0); //todo
            CreateSkill("MAP_MAXX", 0, 0, 0); //todo
            CreateSkill("MAP_MINX", 0, 0, 0); //todo
            CreateSkill("MAP_MAXY", 0, 0, 0); //todo
            CreateSkill("MAP_MINY", 0, 0, 0); //todo
            CreateSkill("MAP_EDGE_X1", 0, 0, 0); //todo
            CreateSkill("MAP_EDGE_X2", 0, 0, 0); //todo
            CreateSkill("MAP_EDGE_Y1", 0, 0, 0); //todo
            CreateSkill("MAP_EDGE_Y2", 0, 0, 0); //todo
            CreateSkill("MAP_SCALE", 0.9f, 0, 1); //todo
            CreateSkill("MAP_MODE", 0, 0, 1); //todo
            CreateSkill("MAP_LAYER", 0, 0, 16); //todo
            CreateSkill("MAP_ROT", 0, 0, 1); //todo
            CreateSkill("COLOR_PLAYER", 7, 0, 0); //todo
            CreateSkill("COLOR_ACTORS", 3, 0, 0); //todo
            CreateSkill("COLOR_THINGS", 13, 0, 0); //todo
            CreateSkill("COLOR_WALLS", 244, 0, 0); //todo
            CreateSkill("COLOR_BORDER", 244, 0, 0); //todo
            CreateSkill("MOUSE_MODE", 0, 0, 2); //todo
            CreateSkill("TOUCH_MODE", 1, 0, 1); //todo
            //#MOUSE_-
            //MOVING ??
            CreateSkill("MOUSE_CALM", 3, 0, 0); //todo <- can apply
            CreateSkill("MOUSE_TIME", 4, 0, 0); //todo <- can apply
            CreateSkill("MICKEY_X", 0, 0, 0); //todo <- can apply
            CreateSkill("MICKEY_Y", 0, 0, 0); //todo <- can apply
            CreateSkill("MOUSE_X", 0, 0, 0); //todo <- can apply
            CreateSkill("MOUSE_Y", 0, 0, 0); //todo <- can apply
            CreateSkill("MOUSE_ANGLE", 0, 0, 0); //todo <- can apply
            CreateSkill("MOUSE_ANGLE", 0, 0, 0); //todo <- can apply
            CreateSkill("TOUCH_DIST", 100, 0, 0); //todo <- can apply
            CreateSkill("TOUCH_STATE", 0, 0, 3); //todo <- can apply
            CreateSkill("JOYSTICK_X", 0, -255, 255); //todo <- can apply
            CreateSkill("JOYSTICK_Y", 0, -255, 255); //todo <- can apply
            CreateSkill("STR_LEN", 0, 0, 0); //todo <- can apply
            CreateSkill("LINES", 0, 0, 0); //todo <- can apply
            CreateSkill("SIZE_Y", 0, 0, 0); //todo <- can apply
            CreateSkill("SOUND_VOL", 0.5f, 0, 1); //todo <- can apply
            CreateSkill("MUSIC_VOL", 0.5f, 0, 1); //todo <- can apply
            CreateSkill("CDAUDIO_VOL", 0.5f, 0, 1); //todo <- can apply
            CreateSkill("CHANNEL", 0, -1, 7); //todo
            CreateSkill("CHANNEL_0", 0, 0, 2); //todo
            CreateSkill("CHANNEL_1", 0, 0, 2); //todo
            CreateSkill("CHANNEL_2", 0, 0, 2); //todo
            CreateSkill("CHANNEL_3", 0, 0, 2); //todo
            CreateSkill("CHANNEL_4", 0, 0, 2); //todo
            CreateSkill("CHANNEL_5", 0, 0, 2); //todo
            CreateSkill("CHANNEL_6", 0, 0, 2); //todo
            CreateSkill("CHANNEL_7", 0, 0, 2); //todo
            CreateSkill("AMBIENT", 0, -1, 1); //todo
            CreateSkill("PLAYER_LIGHT", 1, 0, 1); //todo
            CreateSkill("LIGHT_DIST", 10, 0, 0); //todo
            CreateSkill("DARK_DIST", 0, 0, 0); //todo
            CreateSkill("PLAYER_WIDTH", 1.2f, 0, 0); //todo <- can apply IMPORTANT, collider radius
            CreateSkill("PLAYER_SIZE", 3, 0, 0); //todo <- can apply IMPORTANT, eyes height
            CreateSkill("PLAYER_CLIMB", 1.5f, 0, 0); //todo <- can apply IMPORTANT, step size
            CreateSkill("WALK_PERIOD", 4, 0, 0); //todo <- can apply, bounciness move (blob move)
            CreateSkill("WALK_TIME", 4, 0, 0); //todo <- can apply, bounciness move
            CreateSkill("WAVE_PERIOD", 16, 0, 0); //todo <- can apply, bounciness move
            CreateSkill("WALK", 0, -1, 1); //todo
            CreateSkill("WAVE", 0, -1, 1); //todo
            CreateSkill("PSOUND_VOL", 1, 0, 2); //todo
            CreateSkill("PSOUND_TONE", 1, 0, 4); //
            CreateSkill("PLAYER_VX", 1, 0, 0); //todo <- steering? IMPORTANT
            CreateSkill("PLAYER_VY", 1, 0, 0); //todo <- steering? IMPORTANT
            CreateSkill("PLAYER_VZ", 1, 0, 0); //todo <- jump? IMPORTANT
            CreateSkill("PLAYER_VROT", 1, 0, 0); //todo <- something about rotation IMPORTANT
            CreateSkill("PLAYER_TILT", 1, 0, 0); //todo <- something about rotation IMPORTANT
            CreateSkill("PLAYER_ARC", 1, 0.2f, 2.0f); //todo <- FOV, IMPORTANT. 1.0 = 60 degrees
            CreateSkill("FRICTION", 0.5f, 0.0f, 1.0f); //todo <- move friction IMPORTANT
            CreateSkill("INERTIA", 1f, 0.0f, 0); //todo <- move inertia IMPORTANT
            CreateSkill("SHOOT_RANGE", 500f, 0.0f, 0); //todo <- shoot distance IMPORTANT
            CreateSkill("SHOOT_RANGE", 500f, 0.0f, 0); //todo <- shoot distance IMPORTANT
            //SHOOT_ -
            //    SECTOR ??
        }
    }
}
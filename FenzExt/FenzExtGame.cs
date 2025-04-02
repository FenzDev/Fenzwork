using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenzExt
{
    public static class FenzExtGame
    {
        public static void Run(GameCore core)
        {
            bool alreadyRunning = false;

            using (MGGame mg = new(core))
            {
#if !DEBUG
                bool willContinue = true;

                while (willContinue)
                {
                    try
                    {
                        if (alreadyRunning)
                            return;
                        alreadyRunning = true;
#endif
                        mg.Run();

#if !DEBUG
                        alreadyRunning = false;
                        willContinue = !mg._WillQuit;
                    }
                    catch (Exception ex)
                    {
                        mg._Core.Error(ex);
                    }
            }
#endif
            }
        }

    }
}

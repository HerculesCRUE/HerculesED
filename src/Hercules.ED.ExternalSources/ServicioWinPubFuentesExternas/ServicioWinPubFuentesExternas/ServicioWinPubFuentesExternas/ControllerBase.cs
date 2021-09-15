using System;
using System.Threading;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.OAuth;
using ServicioWinPubFuentesExternas.Utilidades;

namespace ServicioWinPubFuentesExternas
{
    public class ControllerBase
    {
        #region Miembros

        /// <summary>
        /// Intervalo en segundos.
        /// </summary>
        protected readonly int INTERVALO_SEGUNDOS = 30;

        protected readonly ResourceApi mResourceApi;
        protected readonly CommunityApi mCommunityApi;
        protected readonly UserApi mUserApi;
      



        /// <summary>
        /// Token de cancelación para abortar de manera segura los hilos creados.
        /// </summary>
        private CancellationTokenSource mTokenCancelacion;

        protected bool EstaProcesoEnMarcha = false;

        #endregion

        #region Constructor

        public ControllerBase(int pIntervaloSeg)
        {
            INTERVALO_SEGUNDOS = pIntervaloSeg;

            mResourceApi = new ResourceApi(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Config\configOAuth\OAuth_V3.config");
            mCommunityApi = new CommunityApi(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Config\configOAuth\OAuth_V3.config");
            mUserApi = new UserApi(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Config\configOAuth\OAuth_V3.config");


        }

        #endregion

        #region Métodos

        public virtual void RealizarMantenimiento()
        {
            throw new Exception("NOT Implemented");
        }

        protected void ComprobarCancelacionHilo()
        {
            if (mTokenCancelacion != null && mTokenCancelacion.Token != null)
            {
                mTokenCancelacion.Token.ThrowIfCancellationRequested();
            }
        }

        public CancellationTokenSource TokenCancelacion
        {
            get
            {
                return mTokenCancelacion;
            }
            set
            {
                this.mTokenCancelacion = value;
            }
        }



        #endregion
    }
}

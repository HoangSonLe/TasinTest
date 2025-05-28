using Tasin.Website.Common.CommonModels.BaseModels;

namespace Tasin.Website.Common.Helper
{
    public static class CastHelper
    {
        public static Acknowledgement AsAcknowledgement(this Acknowledgement ackT)
        {
            return new Acknowledgement()
            {
                IsSuccess = ackT.IsSuccess,
                ErrorMessageList = ackT.ErrorMessageList,
                SuccessMessageList = ackT.SuccessMessageList
            };
        }
       
        public static Acknowledgement<TResult> AsAcknowledgement<TResult>(this Acknowledgement ackTSource)
        {
            return new Acknowledgement<TResult>()
            {
                IsSuccess = ackTSource.IsSuccess,
                ErrorMessageList = ackTSource.ErrorMessageList,
                SuccessMessageList = ackTSource.SuccessMessageList
            };
        }
    }
}

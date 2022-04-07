using System;
using Epic.OnlineServices;
using Mirage.SocketLayer;
using UnityEngine.Assertions;

namespace Mirage.Sockets.EpicSocket
{
    internal sealed class EpicEndPoint : IEndPoint
    {
        private string _hostProductUserId;
        private ProductUserId _userId;
        public ProductUserId UserId
        {
            get
            {
                if (_userId == null)
                {
                    // can only get id when loaded
                    if (EpicHelper.IsLoaded())
                    {
                        // only call this is host Id is set
                        if (string.IsNullOrEmpty(_hostProductUserId))
                            throw new InvalidOperationException("Host Id is empty");

                        _userId = ProductUserId.FromString(_hostProductUserId);
                    }
                }
                return _userId;
            }
            set
            {
                if (!string.IsNullOrEmpty(_hostProductUserId))
                {
                    Assert.AreEqual(_userId, value);
                }

                _userId = value;
            }
        }

        public EpicEndPoint() { }
        public EpicEndPoint(string hostProductUserId)
        {
            _hostProductUserId = hostProductUserId;
            if (string.IsNullOrEmpty(_hostProductUserId))
                throw new ArgumentException("Host Id is empty");
        }
        private EpicEndPoint(ProductUserId userId)
        {
            _userId = userId;
        }

        IEndPoint IEndPoint.CreateCopy()
        {
            Assert.IsNotNull(UserId);
            return new EpicEndPoint(UserId);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EpicEndPoint other))
                return false;

            if (UserId != null)
            {
                // user Equals because of IEquatable
                return UserId.Equals(other.UserId);
            }
            else if (other.UserId != null)
            {
                // userId is only set on other, so not equal
                return false;
            }
            else
            {
                return _hostProductUserId == other._hostProductUserId;
            }
        }

        public override int GetHashCode()
        {
            // user UserId first, if that is null fallback to string
            if (UserId != null)
            {
                return UserId.GetHashCode();
            }
            else
            {
                return _hostProductUserId.GetHashCode();
            }
        }

        internal void CopyFrom(EpicEndPoint endPoint)
        {
            Assert.IsNotNull(endPoint.UserId);
            UserId = endPoint.UserId;
        }
    }
}


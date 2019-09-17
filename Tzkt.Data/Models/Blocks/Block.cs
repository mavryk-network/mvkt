﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Tzkt.Data.Models
{
    public class Block
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string Hash { get; set; }
        public DateTime Timestamp { get; set; }
        public int ProtocolId { get; set; }

        public int? BakerId { get; set; }
        public int Priority { get; set; }
        public int Validations { get; set; }
        public Operations Operations { get; set; }
        public int? RevelationId { get; set; }

        #region relations
        [ForeignKey(nameof(ProtocolId))]
        public Protocol Protocol { get; set; }

        [ForeignKey(nameof(BakerId))]
        public Delegate Baker { get; set; }

        [ForeignKey(nameof(RevelationId))]
        public NonceRevelationOperation Revelation { get; set; }
        #endregion

        #region indirect relations
        public List<Delegate> ActivatedDelegates { get; set; }
        public List<Delegate> DeactivatedDelegates { get; set; }

        public List<EndorsementOperation> Endorsements { get; set; }

        public List<BallotOperation> Ballots { get; set; }
        public List<ProposalOperation> Proposals { get; set; }

        public List<ActivationOperation> Activations { get; set; }
        public List<DoubleBakingOperation> DoubleBakings { get; set; }
        public List<DoubleEndorsingOperation> DoubleEndorsings { get; set; }
        public List<NonceRevelationOperation> Revelations { get; set; }

        public List<DelegationOperation> Delegations { get; set; }
        public List<OriginationOperation> Originations { get; set; }
        public List<TransactionOperation> Transactions { get; set; }
        public List<RevealOperation> Reveals { get; set; }
        #endregion
    }

    public static class BlockModel
    {
        public static void BuildBlockModel(this ModelBuilder modelBuilder)
        {
            #region indexes
            modelBuilder.Entity<Block>()
                .HasIndex(x => x.Level)
                .IsUnique();

            modelBuilder.Entity<Block>()
                .HasIndex(x => x.Hash)
                .IsUnique();
            #endregion

            #region keys
            modelBuilder.Entity<Block>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Block>()
                .HasAlternateKey(x => x.Level);
            #endregion

            #region props
            modelBuilder.Entity<Block>()
                .Property(x => x.Hash)
                .IsFixedLength(true)
                .HasMaxLength(51)
                .IsRequired();
            #endregion

            #region relations
            modelBuilder.Entity<Block>()
                .HasOne(x => x.Protocol)
                .WithMany(x => x.Blocks)
                .HasForeignKey(x => x.ProtocolId);

            modelBuilder.Entity<Block>()
                .HasOne(x => x.Baker)
                .WithMany(x => x.BakedBlocks)
                .HasForeignKey(x => x.BakerId);

            modelBuilder.Entity<Block>()
                .HasOne(x => x.Revelation)
                .WithOne(x => x.RevealedBlock)
                .HasForeignKey<Block>(x => x.RevelationId)
                .HasPrincipalKey<NonceRevelationOperation>(x => x.RevealedLevel);
            #endregion
        }
    }
}
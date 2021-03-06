﻿using System.Collections.Generic;
using System.Threading.Tasks;
using EHospital.Medications.Model;

namespace EHospital.Medications.BusinessLogic.Contracts
{
    /// <summary>
    /// Provides CRUD operations methods for prescription service.
    /// </summary>
    /// <seealso cref="IService{Prescription}"/>
    public interface IPrescriptionService : IService<Prescription>
    {
        /// <summary>
        /// Gets all prescription details specified by patient identifier
        /// in asynchronous mode. Includes doctor and drug extended details.
        /// </summary>
        /// <param name="patientId">The Patient identifier.</param>
        /// <returns>All patient prescriptions in details.</returns>
        Task<IEnumerable<PrescriptionDetails>> GetPrescriptionsDetails(int patientId);

        /// <summary>
        /// Gets the guide by identifier in asynchronous mode.
        /// Includes drug instruction and doctor's notes.
        /// </summary>
        /// <param name="id">The prescription identifier.</param>
        /// <returns>Drug instruction and doctor's notes.</returns>
        Task<PrescriptionGuide> GetGuideById(int id);

        /// <summary>
        /// Allows to update prescription status manually to historic
        /// in asynchronous mode.
        /// </summary>
        /// <param name="id">The prescription identifier.</param>
        /// <returns>Historic prescription.</returns>
        Task<Prescription> UpdateStatusAsync(int id);
    }
}